using System.Runtime.CompilerServices;
using System.Text.Json;
using ComputerUtils.Logging;

namespace LEDForPi;

public class RBSongPlayer
{
    
    public static MapDifficulty mapSortedByHitTime = new MapDifficulty();
    public static MapDifficulty orgMap = new MapDifficulty();
    public static int currentSuggestedLED = 0;
    public static StripWrapper w = new StripWrapper();
    public static float shipLocation = 0;
    public static DateTime songStartTime = DateTime.Now;
    public static int currentSongId = 0;
    public static int bgColor = 0;
    public static double lastSongShootTime = 0;
    public static int laserShootLED = 0;
    public static double elapsedSeconds => (DateTime.Now - songStartTime).TotalSeconds;
    public static List<int> hitTargets = new List<int>();
    public static List<TargetController> controllers = new();
    public static MapInfo info = new();
    
    
    public static Color currentColor = new(0, 0, 0);
    public static Color lastColor = new(0, 0, 0);
    public static Color currentBgColor = new(0, 0, 0);
    public static Color lastBgColor = new(0, 0, 0);
    public static Color actualColor = new(0, 0, 0);
    public static List<LaserShot> laserShots = new();

    public static void SetSongTime(float songTime)
    {
        // Sync to 35 ms precision
        if (Math.Abs(elapsedSeconds - songTime) < .35) return;
        songStartTime = DateTime.Now - TimeSpan.FromSeconds(songTime);
    }

    public static void LaserShot()
    {
        lastSongShootTime = elapsedSeconds;
        laserShootLED = Utils.LocationToLEDIndex(shipLocation, w);
        laserShots.Add(new LaserShot {
            middleLED = laserShootLED,
            shootTime = lastSongShootTime
        });
    }

    public static void SetShipPos(float pos)
    {
        shipLocation = pos;
    }

    public static void TargetHit(int index)
    {
        hitTargets.Add(index);
    }
    
    /// <summary>
    /// Gets the color of bg or flash at a specific time
    /// </summary>
    /// <param name="time">time to get the color of</param>
    /// <param name="isBg">whether to get the bg or flash color </param>
    /// <returns></returns>
    public static Color GetColorAt(float time, bool isBg = false)
    {
        List<TargetData> colorChangeEvents = orgMap.targets.Where(x => x.type == TargetType.COLORCHANGE).ToList();
        // load bg colors or flash colors
        List<Color> colors = isBg ? info.bgColors : info.colors;
        if (colorChangeEvents.Count == 0 || colorChangeEvents[0].time > time) return colors[0];
        for (int i = 0; i < colorChangeEvents.Count - 1; i++)
        {
            if (colorChangeEvents[i].time <= time && colorChangeEvents[i + 1].time >= time) return colors[colorChangeEvents[i].power / 2];
        }
        return colors[colorChangeEvents[^1].power / 2];
    }
    
    public static void PlaySong(StripWrapper strip, MapDifficulty m)
    {
        laserShots.Clear();
        orgMap = new MapDifficulty(m);
        controllers.Clear();
        currentSongId++;
        hitTargets = new List<int>();
        int thisPlayId = currentSongId + 0;
        w = strip;
        songStartTime = DateTime.Now + TimeSpan.FromSeconds(3);
        MapDifficulty map = new MapDifficulty(m);
        mapSortedByHitTime = new MapDifficulty(map);
        mapSortedByHitTime.targets = mapSortedByHitTime.targets.OrderBy(x => x.time).ToList();
        
        currentColor = GetColorAt(-9999f); // uses color index 0 of the map which should be the default rb color
        currentBgColor = GetColorAt(-9999f, true);
        actualColor = currentBgColor;

        // Order by spawn order
        map.targets = map.targets.OrderBy(x => x.time).ToList();
        map.targets = map.targets.OrderBy(x => x.time - x.lifetime / 2).ToList();

        while (true)
        {
            if (thisPlayId != currentSongId) return;
            float songTime = Convert.ToSingle(elapsedSeconds);
            w.SetAllLED(actualColor.ToInt()); // turn off all leds
            // Spawn targets
            // Instantiate all song cubes to instantiate
            if (map.targets.Count > 0)
            {
                while (songTime >= map.targets[0].time - map.targets[0].lifetime / 2)
                {
                    controllers.Add(new TargetController(map.targets[0], songTime, thisPlayId));
                    map.targets.RemoveAt(0);
                    if (map.targets.Count <= 0) break;
                }
            }
            
            // Show ship location
            double timeSinceLastShoot = elapsedSeconds - lastSongShootTime;
            double brightness = Math.Clamp(1 - timeSinceLastShoot * timeSinceLastShoot * 4f, .3, 1);
    
            //UpdateSuggestedMovement(songTime);
            w.SetLED(Utils.LocationToLEDIndex(shipLocation, w), 0xff33b4, brightness);
            
            // Add 3 wide laser
            for (int l = 0; l < laserShots.Count; l++)
            {
                if (laserShots[l].Update(w))
                {
                    laserShots.RemoveAt(l);
                    l--;
                }
            } 
            // Update targets
            for (int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i].Update(songTime, w, currentSuggestedLED))
                {
                    controllers.RemoveAt(i);
                    i--;
                }
            }
            w.Render();
            if (controllers.Count <= 0 && map.targets.Count <= 0)
            {
                break;
            }
        }
    }
    
    public static void UpdateSuggestedMovement(float time)
    {
        float pos = GetSuggestedPositionForTime(time);
        mapSortedByHitTime.targets.RemoveAll(x => x.time < time - 1f); // remove all targets that are too old
        currentSuggestedLED = Utils.LocationToLEDIndex(pos, w);
        w.SetLED(currentSuggestedLED, 0xFF00FF);
    }

    public static float GetSuggestedPositionForTime(float time)
    {
        TargetData targetBefore = new TargetData();
        TargetData targetAfter = new TargetData();
        bool firstCube = true;
        for (int i = 0; i < mapSortedByHitTime.targets.Count; i++)
        {
            if(mapSortedByHitTime.targets[i].type != TargetType.NORMAL && mapSortedByHitTime.targets[i].type != TargetType.MANUAL) continue;
            if (firstCube && mapSortedByHitTime.targets[i].time > time)
            {
                targetBefore = mapSortedByHitTime.targets[i];
                targetAfter = mapSortedByHitTime.targets[i];
                break;
            }
            firstCube = false;
            if(mapSortedByHitTime.targets[i].time <= time)
            {
                targetBefore = mapSortedByHitTime.targets[i];
                for (int k = i; k < mapSortedByHitTime.targets.Count; k++)
                {
                    if (k + 1 >= mapSortedByHitTime.targets.Count)
                    {
                        targetAfter =  mapSortedByHitTime.targets[i];
                        break;
                    }
                    if(mapSortedByHitTime.targets[k + 1].type != TargetType.NORMAL && mapSortedByHitTime.targets[k + 1].type != TargetType.MANUAL) continue;
                    targetAfter = mapSortedByHitTime.targets[k + 1];
                    break;
                }
            }
        }

        float totalDiff = targetAfter.time - targetBefore.time;
        if (totalDiff > .2f) totalDiff = .2f;
        float progress = 0f;
        if(totalDiff != 0f)
        {
            progress = (time - targetBefore.time) / totalDiff;
        }

        if (progress > 1f) progress = 1f;
        progress = CodeAnimationHelpers.EaseOutCurve(progress);
        float lerpPositionX = Utils.Lerp(targetBefore.location, targetAfter.location, progress);
        return lerpPositionX;
    }


    public static void SetBG(int rBg)
    {
        bgColor = rBg;
    }
}