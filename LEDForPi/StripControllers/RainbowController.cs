using System.Reflection.Metadata;
using ComputerUtils.Logging;
using LEDForPi.Strips;

namespace LEDForPi;

public class RainbowController : BasicStripController, IStripController
{
    VirtualStrip w = new();

    public RainbowController(VirtualStrip w)
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
        for(int i=0; i<w.LEDCount; i++) { 
            double pixelHue = AnimationSettings.hue + (i * 720.0 / w.LEDCount);
            w.SetLED(i, ColorUtils.HsvToRgb(pixelHue, 1, 1));
        }

        AnimationSettings.hue += manager.deltaTime * (AnimationSettings.step / 10.0);
        AnimationSettings.hue %= 360;
        w.SetBrightness(AnimationSettings.brightness);
    }
}