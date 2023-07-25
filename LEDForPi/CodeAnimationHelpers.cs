namespace LEDForPi;

public class CodeAnimationHelpers
{
    
    public static float EaseOutCurve(float t)
    {
        return Convert.ToSingle(Math.Sqrt(1 - Math.Pow(t - 1, 2)));
    }
}