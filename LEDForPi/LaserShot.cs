namespace LEDForPi;

public class LaserShot
{
    public int middleLED { get; set; } = 0;
    public double shootTime { get; set; } = 0;

    public bool Update(StripWrapper w)
    {
        double timeSinceLastShoot = RBSongPlayer.elapsedSeconds - shootTime;
        double laserBrightness = Math.Clamp(1 - timeSinceLastShoot * timeSinceLastShoot * 4f, 0, 1);
        if (laserBrightness > 0)
        {
            w.SetLED(middleLED, 0xFFFFFF, laserBrightness);
            if(middleLED - 1 >= 0) w.SetLED(middleLED - 1, 0xFFFFFF, laserBrightness);
            if(middleLED + 1 < w.LEDCount) w.SetLED(middleLED + 1, 0xFFFFFF, laserBrightness);
        }
        if(laserBrightness <= 0) return true;
        return false;
    }
}