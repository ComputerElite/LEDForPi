namespace LEDForPi;

public class Utils
{

    public static int LocationToLEDIndex(float location, StripWrapper stripWrapper)
    {
        return stripWrapper.LEDCount - (int)Math.Round((location + 1) / 2f * stripWrapper.LEDCount);
    }
    
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}