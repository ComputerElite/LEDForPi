//The default settings uses a frequency of 800000 Hz and the DMA channel 10.

using System.Device.Gpio;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using LEDForPi;
using NetCoreAudio;
using rpi_ws281x;

StripWrapper w = new StripWrapper();
w.Init(30, Pin.Gpio21);

Player p = new Player();
p.Play("audio.wav");
int LED = 0;
int dir = 1;

List<TargetController> controllers = new();
DateTime songStartTime = DateTime.Now;
MapDifficulty map = JsonSerializer.Deserialize<MapDifficulty>(File.ReadAllText("map.json"));

// Order by spawn order
map.targets = map.targets.OrderBy(x => x.time - x.lifetime / 2).ToList();
MapDifficulty mapSortedByHitTime = new MapDifficulty(map);
map.targets = map.targets.OrderBy(x => x.time).ToList();

int currentSuggestedLED = 0;

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

void UpdateSuggestedMovement(float time)
{
    float pos = GetSuggestedPositionForTime(time);
    mapSortedByHitTime.targets.RemoveAll(x => x.time < time - 1f); // remove all targets that are too old
    currentSuggestedLED = TargetController.LocationToLEDIndex(pos, w);
    w.SetLED(currentSuggestedLED, 0xFF00FF);
}

float GetSuggestedPositionForTime(float time)
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
    float lerpPositionX = Lerp(targetBefore.location, targetAfter.location, progress);
    return lerpPositionX;
}

float Lerp(float a, float b, float t)
{
    return a + (b - a) * t;
}

public class TargetController
{
    public TargetData data;

    public float instantiateTime;
    public TargetController(TargetData d, float songTime)
    {
        data = d;
        instantiateTime = songTime;
    }

    public static int LocationToLEDIndex(float location, StripWrapper stripWrapper)
    {
        return stripWrapper.LEDCount - (int)Math.Round((location + 1) / 2f * stripWrapper.LEDCount);
    }

    public bool Update(float songTime, StripWrapper stripWrapper, int currentSuggestedLED)
    {
        if (data.type != TargetType.NORMAL) return true;
        float progress = -1 + (songTime - instantiateTime) / data.lifetime * 2;
        if (progress > 1)
        {
            return true;
        }

        int led = LocationToLEDIndex(data.location, stripWrapper);
        int color = 0xFFFFFF;
        if (progress > 0)
        {
            // Lerp between 0xFFFFFF and 0xFF0000
            float redAmount = Math.Clamp(progress * 10, 0, 1);
            color = 0xFF0000 + (int)Math.Round((1 - redAmount) * 0xFF) * 0x100 + (int)Math.Round((1 - redAmount) * 0xFF);
        }

        double brightness = 0;
        if(progress < 0) brightness = 1 + progress;
        else brightness = 1 - progress;
        brightness = brightness * brightness;
        stripWrapper.SetLED(led, currentSuggestedLED == led ? 0xFF00FF : color, brightness);
        return false;
    }
}