namespace LEDForPi;

public class CodeAnimationHelpers
{
    
    public static float EaseOutCurve(float t)
    {
        return Convert.ToSingle(Math.Sqrt(1 - Math.Pow(t - 1, 2)));
    }

    public static float EaseInOutCurve(float t)
    {
        if (t <= 0.5f)
            return 2.0f * t * t;
        t -= 0.5f;
        return 2.0f * t * (1.0f - t) + 0.5f;
    }
}