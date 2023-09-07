using LEDForPi.RBExtras;
using LEDForPi.Strips;

namespace LEDForPi;

public class LaserShot
{
    public int middleLED { get; set; } = 0;
    public double shootTime { get; set; } = 0;

    public bool Update(VirtualStrip w, RBStripController controller)
    {
        if (!RBSongPlayerConfig.enableLaser) return true;
        double timeSinceLastShoot = controller.elapsedSeconds - shootTime;
        if(timeSinceLastShoot > 1) return true;
        
        double laserBrightness = Math.Clamp(Math.Pow(1 - timeSinceLastShoot, 8), 0, 1);
        int color = Color.Lerp(new Color(1f, 1f, 1f) * (float)laserBrightness, controller.actualColor, laserBrightness < 0.33f ? 1 - (float)laserBrightness * 3 : 0).ToInt();
        if (laserBrightness > 0)
        {
            w.SetLED(middleLED, color);
            if(middleLED - 1 >= 0) w.SetLED(middleLED - 1, color);
            if(middleLED + 1 < w.LEDCount) w.SetLED(middleLED + 1, color);
        }
        return false;
    }
}