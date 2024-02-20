using LEDForPi.Strips;

namespace LEDForPi;

public class StaticController : BasicStripController, IStripController
{
    VirtualStrip w = new();
    private double hue = 0;

    public StaticController(VirtualStrip w)
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
        w.SetAllLED(AnimationSettings.color0);
        w.SetBrightness(AnimationSettings.brightness);
    }
}