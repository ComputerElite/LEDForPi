namespace LEDForPi;

public class Utils
{

    public static int LocationToLEDIndex(float location, StripWrapper stripWrapper)
    {
        if (RBSongPlayer.flipped)
        {
            return (int)((location + 1) * (stripWrapper.LEDCount - 1) / 2f);
        }
        return (stripWrapper.LEDCount - 1) - (int)((location + 1) * (stripWrapper.LEDCount - 1) / 2f) - (RBSongPlayer.flipped ? stripWrapper.LEDCount + 1 : 0);
    }
    
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static double PerlinNoise(float x)
    {
        return Math.Sin(x + 2) * Math.Sin(2 * x + 1) * -Math.Sin(2.5f * x + 1);
    }
}