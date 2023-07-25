using System.Runtime.CompilerServices;
using System.Text.Json;

namespace LEDForPi;

public class RBSongPlayer
{
    
    public static MapDifficulty mapSortedByHitTime = new MapDifficulty();
    public static int currentSuggestedLED = 0;
    public static StripWrapper w = new StripWrapper();
    public static void PlaySong(StripWrapper strip, MapDifficulty m)
    {
        w = strip;
        List<TargetController> controllers = new();
        DateTime songStartTime = DateTime.Now;
        MapDifficulty map = new MapDifficulty(m);
        mapSortedByHitTime = new MapDifficulty(map);

// Order by spawn order
        map.targets = map.targets.OrderBy(x => x.time - x.lifetime / 2).ToList();
        map.targets = map.targets.OrderBy(x => x.time).ToList();

        while (true)
        {
            double elapsedSeconds = (DateTime.Now - songStartTime).TotalSeconds;
            float songTime = Convert.ToSingle(elapsedSeconds);
            w.SetAllLED(0x000000); // turn off all leds
            // Spawn targets
            // Instantiate all song cubes to instantiate
            if (map.targets.Count > 0)
            {
                while (songTime >= map.targets[0].time - map.targets[0].lifetime / 2)
                {
                    controllers.Add(new TargetController(map.targets[0], songTime));
                    map.targets.RemoveAt(0);
                    if (map.targets.Count <= 0) break;
                }
            }
    
            UpdateSuggestedMovement(songTime);
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

    
}