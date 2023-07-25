namespace LEDForPi;

public class TargetController
{
    public TargetData data;

    public float instantiateTime;
    public TargetController(TargetData d, float songTime)
    {
        data = d;
        instantiateTime = songTime;
    }


    public bool Update(float songTime, StripWrapper stripWrapper, int currentSuggestedLED)
    {
        if (data.type != TargetType.NORMAL) return true;
        float progress = -1 + (songTime - instantiateTime) / data.lifetime * 2;
        if (progress > 1)
        {
            return true;
        }

        int led = Utils.LocationToLEDIndex(data.location, stripWrapper);
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