namespace LEDForPi;

public class Utils
{

    public static int LocationToLEDIndex(float location, StripWrapper stripWrapper)
    {
        return (stripWrapper.LEDCount - 1) - (int)((location + 1) * (stripWrapper.LEDCount - 1) / 2f);
    }
    
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}