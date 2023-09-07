using ComputerUtils.Logging;
using LEDForPi.RBExtras;
using LEDForPi.Strips;

namespace LEDForPi;

public class RBStripController : BasicStripController, IStripController
{
    public MapDifficulty mapSortedByHitTime = new MapDifficulty();
    public MapDifficulty orgMap = new MapDifficulty();
    public MapDifficulty map = new MapDifficulty();
    
    public List<LaserShot> laserShots = new();
    public float shipLocation = 0;
    public VirtualStrip w = new();
    public DateTime songStartTime = DateTime.Now;
    public DateTime lastUpdate = DateTime.Now;
    public float deltaTime => (float)(DateTime.Now - lastUpdate).TotalSeconds * speed;
    public double elapsedSeconds => (DateTime.Now - songStartTime).TotalSeconds * speed;

    public double lastSongShootTime = 0;
    public List<int> hitTargets = new List<int>();
    public List<TargetController> controllers = new();
    private int currentSuggestedLED = 0;
    
    public Color currentColor = new(0, 0, 0);
    public Color lastColor = new(0, 0, 0);
    public Color currentBgColor = new(0, 0, 0);
    public Color lastBgColor = new(0, 0, 0);
    public Color actualColor = new(0, 0, 0);
    public float waveIntensity = 0;
    public float waveoffset = 0;
    public float waveDirection = 1f;

    public float speed = 1f;

    public int thisPlayId = 0;
    
    public List<double> fps = new();
    
    public void SetSongTime(float songTime)
    {
        // Sync to 15 ms precision
        double deltaTime = Math.Abs(elapsedSeconds - songTime);
        //Logger.Log(deltaTime * 1000 + " ms off");
        if (deltaTime < .015) return;
        songStartTime = DateTime.Now - TimeSpan.FromSeconds(songTime) / speed;
    }
    
    public void SetSpeed(float s)
    {
        speed = s;
    }

    public void LaserShot(bool instaRender = false)
    {
        lastSongShootTime = elapsedSeconds;
        int laserShootLED = Utils.LocationToLEDIndex(shipLocation, w);
        laserShots.Add(new LaserShot {
            middleLED = laserShootLED,
            shootTime = lastSongShootTime
        });
        if (instaRender) laserShots[^1].Update(w, this);
    }

    public void SetShipPos(float pos)
    {
        shipLocation = Math.Clamp(pos, -1, 1);
    }

    public void TargetHit(int index)
    {
        hitTargets.Add(index);
    }
    
    /// <summary>
    /// Gets the color of bg or flash at a specific time
    /// </summary>
    /// <param name="time">time to get the color of</param>
    /// <param name="isBg">whether to get the bg or flash color </param>
    /// <returns></returns>
    public Color GetColorAt(float time, bool isBg = false)
    {
        List<TargetData> colorChangeEvents = orgMap.targets.Where(x => x.type == TargetType.COLORCHANGE).ToList();
        // load bg colors or flash colors
        List<Color> colors = isBg ? RBSongPlayer.info.bgColors : RBSongPlayer.info.colors;
        if (colorChangeEvents.Count == 0 || colorChangeEvents[0].time > time) return colors[0];
        for (int i = 0; i < colorChangeEvents.Count - 1; i++)
        {
            if (colorChangeEvents[i].time <= time && colorChangeEvents[i + 1].time >= time) return colors[colorChangeEvents[i].power / 2];
        }
        return colors[colorChangeEvents[^1].power / 2];
    }

    public void InitSong(VirtualStrip strip, MapDifficulty m)
    {
        laserShots.Clear();
        if(RBSongPlayerConfig.playfieldSize == -1) RBSongPlayerConfig.playfieldSize = strip.LEDCount;
        waveoffset = 0;
        waveIntensity = 0;
        orgMap = new MapDifficulty(m);
        controllers.Clear();
        RBSongPlayer.currentSongId++;
        hitTargets = new List<int>();
        thisPlayId = RBSongPlayer.currentSongId + 0;
        w = strip;
        songStartTime = DateTime.Now + TimeSpan.FromSeconds(3); 
        map = new MapDifficulty(m);
        mapSortedByHitTime = new MapDifficulty(map);
        mapSortedByHitTime.targets = mapSortedByHitTime.targets.OrderBy(x => x.time).ToList();
        
        currentColor = GetColorAt(-9999f); // uses color index 0 of the map which should be the default rb color
        currentBgColor = GetColorAt(-9999f, true);
        actualColor = currentBgColor;

        // Order by spawn order
        map = OrderMapAndAddIndex(map);
        
        
        if (!RBSongPlayer.useGame)
        {
            if (RBSongPlayer.replay != null)
            {
                RBSongPlayer.replay.PlayFirstFrame();
            }
            else
            {
                speed = 1;
            }
        }
    }

    private MapDifficulty OrderMapAndAddIndex(MapDifficulty orgMap)
    {
        MapDifficulty map = new MapDifficulty(orgMap);
        map.targets = map.targets.OrderBy(x => x.time).ToList();
        int cubeIndex = 0;
        for (int i = 0; i < map.targets.Count; i++)
        {
			if (map.targets[i].type != TargetType.NORMAL && map.targets[i].type != TargetType.MANUAL)
            {
                // We only have to do more stuff to normal and manual cubes
                continue;
		    }   
			map.targets[i].index = cubeIndex;
			cubeIndex++;
            // Time to mark the cube as shootable (visually)
			map.targets[i].markTime = -10f;
            if (i > 0)
            {
                for(int r = 1; r <= i; r++)
                {
                    if (map.targets[i - r].type != TargetType.NORMAL && map.targets[i - r].type != TargetType.MANUAL)
                    {
                        continue;
                    }
                    map.targets[i].markTime = map.targets[i - r].time;
                    break;
                }
            }
            // Mark the cube as not shootable after shoot time (visually)
            map.targets[i].unMarkTime = map.targets[i].time;
		}
        // Order by spawn order
        map.targets = SortMap(map.targets);
        return map;
    }
    
    public List<TargetData> SortMap(List<TargetData> toSort)
    {
        List<TargetData> sorted = new List<TargetData>(toSort);

        // Reset drive manager so currentDrive is -1 and thus no drive is active
        sorted = sorted.OrderBy(x => x.time).ToList();

        // Order by spawn order
        sorted = sorted.OrderBy(x => x.time - x.lifetime / 2).ToList();

        Dictionary<float, List<TargetData>> spawnTimes = new Dictionary<float, List<TargetData>>();
        for (int i = 0; i < sorted.Count; i++)
        {
            float instantiateTime = sorted[i].time - sorted[i].lifetime / 2;
            if (!spawnTimes.ContainsKey(instantiateTime))
            {
                spawnTimes.Add(instantiateTime, new List<TargetData>());
            }
            spawnTimes[instantiateTime].Add(sorted[i]);
        }
        sorted = new List<TargetData>();
        for (int i = 0; i < spawnTimes.Count; i++)
        {
            spawnTimes[spawnTimes.Keys.ElementAt(i)].Sort((x, y) => x.location.CompareTo(y.location));
            spawnTimes[spawnTimes.Keys.ElementAt(i)].Sort((x, y) => x.time.CompareTo(y.time));
            sorted.AddRange(spawnTimes[spawnTimes.Keys.ElementAt(i)]);
        }
        return sorted;
    }

    public void UpdateSuggestedMovement(float time)
    {
        float pos = GetSuggestedPositionForTime(time);
        //mapSortedByHitTime.targets.RemoveAll(x => x.time < time - 20f); // remove all targets that are really old
        SetShipPos(pos);
        return;
        currentSuggestedLED = Utils.LocationToLEDIndex(pos, w);
        w.SetLED(currentSuggestedLED, 0xFF00FF);
    }

    public float GetSuggestedPositionForTime(float time)
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
        //if (totalDiff > .2f) totalDiff = .2f;
        float progress = 0f;
        if(totalDiff != 0f)
        {
            progress = (time - targetBefore.time) / totalDiff;
        }

        if (progress > 1f) progress = 1f;
        progress = CodeAnimationHelpers.EaseInOutCurve(progress);
        float lerpPositionX = Utils.Lerp(targetBefore.location, targetAfter.location, progress);
        return lerpPositionX;
    }

    public void Update()
    {
        try
        {
            if (thisPlayId != RBSongPlayer.currentSongId)
            {
                Logger.Log("Song changed, destroying");
                manager.Destroy(this);
                return;
            }
            float songTime = Convert.ToSingle(elapsedSeconds);
            w.SetAllLED(RBSongPlayerConfig.enableBGColor ? actualColor.ToInt() : 0x000000); // set bg color
            // Spawn targets
            // Instantiate all song cubes to instantiate
            if (map.targets.Count > 0)
            {
                while (songTime >= map.targets[0].time - map.targets[0].lifetime / 2)
                {
                    controllers.Add(new TargetController(map.targets[0], songTime, thisPlayId, this));
                    map.targets.RemoveAt(0);
                    if (map.targets.Count <= 0) break;
                }
            }
        
            // Show ship location
            double timeSinceLastShoot = elapsedSeconds - lastSongShootTime;
            double brightness = Math.Clamp(1 - timeSinceLastShoot * timeSinceLastShoot * 4f, .3, 1);

            // Add 3 wide laser
            for (int l = 0; l < laserShots.Count; l++)
            {
                if (laserShots[l].Update(w, this))
                {
                    laserShots.RemoveAt(l);
                    l--;
                }
            }

            if (!RBSongPlayer.useGame)
            {
                if (RBSongPlayer.replay != null)
                {
                    RBSongPlayer.replay.PlayFrame();
                }
                else
                {
                    UpdateSuggestedMovement(songTime);
                }
            }
            if(RBSongPlayerConfig.enableShip) w.SetLED(Utils.LocationToLEDIndex(shipLocation, w), 0xff33b4, RBSongPlayerConfig.brightenShipOnShoot ? brightness : 1);
        
            // Update targets
            for (int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i].Update(songTime, w, currentSuggestedLED, this))
                {
                    controllers.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < w.LEDCount; i++)
            {
                double change = Math.Abs(Math.Sin((waveoffset + i * (RBSongPlayerConfig.flipped ? -1 : 1)) * .3f) * waveIntensity);
                w.SetLEDBrightness(i, 1 - Math.Clamp(change, 0, 1));
            }
            fps.Add(1.0 / deltaTime);

            if (fps.Count > 100)
            {
                //Logger.Log(fps.Average() + " FPS");
                fps.Clear();
            }
            w.Render();
            if (controllers.Count <= 0 && map.targets.Count <= 0)
            {
                manager.Destroy(this);
            }
        }
        catch (Exception e)
        {
            Logger.Log("Frame dropped: " + e);
        }
        
        lastUpdate = DateTime.Now;
    }

    public void SetStripControllerManager(StripControllerManager manager)
    {
        this.manager = manager;
        RBSongPlayer.mostRecentStripController = this;
    }
}