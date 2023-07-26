using System.Numerics;
using ComputerUtils.Logging;

namespace LEDForPi;

public class TargetController
{
    public TargetData data;

    public float instantiateTime;
    public int mySongPlayId = 0;
    public bool missed = false;
    public TargetController(TargetData d, float songTime, int songPlayId)
    {
        data = d;
        mySongPlayId = songPlayId;
        instantiateTime = songTime;
        shakeIntensity = data.power / 10f;
        shakeSpeed = data.height;
    }

    float sharedSamplingPoint = 0;
    
    public float shakeIntensity = -10;
    public float shakeSpeed = -10;


    /// <summary>
    /// Does everything to display the LED
    /// </summary>
    /// <returns>Whether to destroy this controller or not</returns>
    public bool Update(float songTime, StripWrapper stripWrapper, int currentSuggestedLED)
    {
        if(mySongPlayId != RBSongPlayer.currentSongId) return true;
        
        
        float progress = -1 + (songTime - instantiateTime) / data.lifetime * 2;
        
        // Handle flash target type
        if (data.type == TargetType.FLASH)
        {
            if (progress < 0) return false;
            progress = 1 - progress;
            double alpha = Math.Pow(progress, 5);
            if (alpha > 1) alpha = 1;
            RBSongPlayer.actualColor = Color.Lerp(RBSongPlayer.currentBgColor, RBSongPlayer.currentColor, Convert.ToSingle(alpha));
            return false;
        }
        // Handle Color change target type
        if (data.type == TargetType.COLORCHANGE)
        {
            Color newColor = data.power == -2 ? new Color(.78f, 0f, .12f) : RBSongPlayer.info.colors[data.power / 2];
            Color newColorBg = data.power == -2 ? new Color(.13f, 0f, .02f) : RBSongPlayer.info.bgColors[data.power / 2];
            if (progress >= 1)
            {
                RBSongPlayer.lastColor = newColor;
                RBSongPlayer.currentColor = newColor;
                RBSongPlayer.lastBgColor = newColorBg;
                RBSongPlayer.currentBgColor = newColorBg;
                return true;
            }
            if (progress < 0) return false;
            float blend = CodeAnimationHelpers.EaseInOutCurve(progress);
            RBSongPlayer.currentColor = Color.Lerp(RBSongPlayer.lastColor, newColor, blend);
            RBSongPlayer.currentBgColor = Color.Lerp(RBSongPlayer.lastBgColor, newColorBg, blend);
            return false;
        }
        if(data.type == TargetType.SHAKE)
        {
            if (progress < 0) return false;
            if (progress > 1)
            {
                return true;
            }

            // intensity between 100 and 1000
            float currentIntensity = (1f - CodeAnimationHelpers.EaseOutCurve(progress));

            RBSongPlayer.waveIntensity = currentIntensity;
            // shake speed 0 - 100
            RBSongPlayer.waveoffset += RBSongPlayer.deltaTime * shakeSpeed * 20f;
            return false;
        }
        if (data.type != TargetType.NORMAL) return true;
        if (RBSongPlayer.hitTargets.Contains(data.index)) return true;
        
        if (progress > 1)
        {
            return true;
        }

        missed = progress > 0;

        int led = Utils.LocationToLEDIndex(data.location, stripWrapper);
        int color = 0xFFFFFF;
        // Check if we are the next target
        bool anythingEarlier = RBSongPlayer.controllers.Where(x => !x.missed && x.data.type == TargetType.NORMAL).Any(x => x.data.index != data.index && x.data.time < data.time);
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
        if (led == Utils.LocationToLEDIndex(RBSongPlayer.shipLocation, stripWrapper))
        {
            if (songTime % .05f < .025f)
                stripWrapper.SetLED(led, currentSuggestedLED == led ? 0xFF00FF : color, brightness);
            else return false;
        }
        stripWrapper.SetLED(led, currentSuggestedLED == led ? 0xFF00FF : color, brightness);
        return false;
    }
}