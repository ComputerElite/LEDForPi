using LEDForPi.Strips;

namespace LEDForPi;

public class RainbowStaticController : BasicStripController, IStripController
{
    VirtualStrip w = new();

    public RainbowStaticController(VirtualStrip w)
    {
        this.w = w;
    }

    public List<IStrip> GetStrips()
    {
        return new List<IStrip> {w};
    }

    public void OnEnable()
    {
        manager.JustMe(this);
    }

    public void Update()
    {
        w.SetAllLED(ColorUtils.HsvToRgb(AnimationSettings.hue, 1, 1));

        AnimationSettings.hue += manager.deltaTime * (AnimationSettings.step / 10.0);
        AnimationSettings.hue %= 360;
        w.SetBrightness(AnimationSettings.brightness);
    }
}