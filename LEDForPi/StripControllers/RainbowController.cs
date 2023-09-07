using System.Reflection.Metadata;
using LEDForPi.Strips;

namespace LEDForPi;

public class RainbowController : BasicStripController, IStripController
{
    VirtualStrip w = new();
    private long hue = 0;

    public RainbowController(VirtualStrip w)
    {
        this.w = w;
    }
    public void Update()
    {
        /*
        for(int i=0; i<w.LEDCount; i++) { 
            long pixelHue = hue + (i * 65536L / w.LEDCount);
            strip.setPixelColor(i, strip.gamma32(strip.ColorHSV(pixelHue)));
        }
        hue += static_cast<long>(GetStepForTime() / 4.0);
        w.Render();
        */
    }
}