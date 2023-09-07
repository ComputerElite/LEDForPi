using System.Numerics;
using ComputerUtils.Logging;
using LEDForPi.Strips;

namespace LEDForPi.RBExtras;

public class TargetController
{
    public TargetData data;

    public float instantiateTime;
    public int mySongPlayId = 0;
    public bool missed = false;
    public TargetController(TargetData d, float songTime, int songPlayId, RBStripController controller)
    {
        data = d;
        mySongPlayId = songPlayId;
        instantiateTime = songTime;
        shakeIntensity = data.power / 10f;
        shakeSpeed = data.height;

        if (data.type == TargetType.COLORCHANGE && RBSongPlayerConfig.flipShakeDirectionOnColorChange) controller.waveDirection *= -1;
    }

    float sharedSamplingPoint = 0;
    
    public float shakeIntensity = -10;
    public float shakeSpeed = -10;

    private bool laserSpawned = false;


    /// <summary>
    /// Does everything to display the LED
    /// </summary>
    /// <returns>Whether to destroy this controller or not</returns>
    public bool Update(float songTime, VirtualStrip stripWrapper, int currentSuggestedLED, RBStripController controller)
    {
        if(mySongPlayId != RBSongPlayer.currentSongId) return true;
        
        
        float progress = -1 + (songTime - instantiateTime) / data.lifetime * 2;
        
        // Handle flash target type
        if (data.type == TargetType.FLASH)
        {
            if (!RBSongPlayerConfig.enableFlashes)
            {
                controller.actualColor = controller.currentBgColor;
                return false;
            }
            if (progress < 0) return false;
            progress = 1 - progress;
            double alpha = Math.Pow(progress, 5);
            if (alpha > 1) alpha = 1;
            controller.actualColor = Color.Lerp(controller.currentBgColor, controller.currentColor, Convert.ToSingle(alpha));
            return false;
        }
        // Handle Color change target type
        if (data.type == TargetType.COLORCHANGE)
        {
            if (!RBSongPlayerConfig.enableColorChanges) return false;
            Color newColor = data.power == -2 ? new Color(.78f, 0f, .12f) : RBSongPlayer.info.colors[data.power / 2];
            Color newColorBg = data.power == -2 ? new Color(.13f, 0f, .02f) : RBSongPlayer.info.bgColors[data.power / 2];
            if (progress >= 1)
            {
                controller.lastColor = newColor;
                controller.currentColor = newColor;
                controller.lastBgColor = newColorBg;
                controller.currentBgColor = newColorBg;
                return true;
            }
            if (progress < 0) return false;
            float blend = CodeAnimationHelpers.EaseInOutCurve(progress);
            controller.currentColor = Color.Lerp(controller.lastColor, newColor, blend);
            controller.currentBgColor = Color.Lerp(controller.lastBgColor, newColorBg, blend);
            return false;
        }
        if(data.type == TargetType.SHAKE)
        {
            if (!RBSongPlayerConfig.enableShakes)
            {
                controller.waveIntensity = 0f;
                return false;
            }
            if (progress < 0) return false;
            if (progress > 1)
            {
                return true;
            }

            // intensity between 100 and 1000
            float currentIntensity = (1f - CodeAnimationHelpers.EaseOutCurve(progress));

            controller.waveIntensity = currentIntensity;
            // shake speed 0 - 100
            controller.waveoffset += controller.deltaTime * shakeSpeed * RBSongPlayerConfig.waveSpeedMultiplier * controller.waveDirection;
            return false;
        }
        if (data.type != TargetType.NORMAL) return true;
        if (controller.hitTargets.Contains(data.index)) return true;
        
        if (progress > 1)
        {
            return true;
        }
        
        missed = progress > 0;
        if (missed)
        {
            if (!RBSongPlayer.useGame && !laserSpawned)
            {
                // Do stuff based on replay
                if (RBSongPlayer.replay == null)
                {
                    // On Auto play we obviously hit
                    controller.LaserShot(true);
                    laserSpawned = true;
                    return true;
                }
                else
                {
                    RBReplayTarget t = RBSongPlayer.replay.GetCubeData(data);
                    controller.LaserShot(true);
                    laserSpawned = true;
                    if (t.h) return true;
                }
            }
        }
        
        if (!RBSongPlayerConfig.enableCubes) return false;


        int led = Utils.LocationToLEDIndex(data.location, stripWrapper);
        int color = 0xFFFFFF;
        // Check if we are the next target
        bool anythingEarlier = controller.controllers.Where(x => !x.missed && x.data.type == TargetType.NORMAL).Any(x => x.data.index != data.index && x.data.time < data.time);
        if (!anythingEarlier)
        {
            color = 0x00EEFF;
        }
        if (progress > 0)
        {
            // Lerp between 0xFFFFFF and 0xFF0000
            float redAmount = Math.Clamp(progress * 10, 0, 1);
            color = 0xFF0000 + (int)Math.Round((1 - redAmount) * 0xFF) * 0x100 + (int)Math.Round((1 - redAmount) * 0xFF);
        }

        double brightness = Math.Clamp(1 - Math.Pow(progress, data.power), 0, 1);
        brightness *= brightness;
        if (led == Utils.LocationToLEDIndex(controller.shipLocation, stripWrapper) && RBSongPlayerConfig.enableShip)
        {
            if (songTime % RBSongPlayerConfig.flashTimeCubeShipMs < RBSongPlayerConfig.flashTimeCubeShipMs / 2)
                stripWrapper.SetLED(led, currentSuggestedLED == led ? 0xFF00FF : color, brightness);
            else return false;
        }
        stripWrapper.SetLED(led, currentSuggestedLED == led ? 0xFF00FF : color, brightness);
        return false;
    }
}