using ComputerUtils.Logging;
using rpi_ws281x;

namespace LEDForPi.Strips;

public class VirtualStrip : IStrip
{
    List<StripWrapper> strips = new();
    public int LEDCount = 0;
    public Dictionary<int, System.Drawing.Color> colors = new();
    public Dictionary<int, System.Drawing.Color> displayedColors = new();
    
    public Dictionary<int, int> ledToStrip = new();
    public void Init(List<StripRepresentation> strips)
    {
        int count = 0;
        LEDCount = 0;
        this.strips.Clear();
        colors.Clear();
        displayedColors.Clear();
        ledToStrip.Clear();
        foreach (StripRepresentation stripRepresentation in strips)
        {
            StripWrapper strip = new();
            strip.isVirtual = stripRepresentation.isVirtual;
            strip.isReversed = stripRepresentation.reversed;
            strip.Init(stripRepresentation.ledCount, stripRepresentation.pin);
            strip.LEDStartIndex = LEDCount;
            LEDCount += stripRepresentation.ledCount;
            this.strips.Add(strip);
            for (int i = 0; i < stripRepresentation.ledCount; i++)
            {
                ledToStrip[strip.LEDStartIndex + i] = count;
            }

            count++;
        }
        Logger.Log("LED Count: " + LEDCount);
    }

    public int GetLEDCount()
    {
        return LEDCount;
    }

    public Dictionary<int, System.Drawing.Color> GetDisplayedColors()
    {
        return displayedColors;
    }

    public void SetBrightness(double brightness)
    {
        foreach (KeyValuePair<int,System.Drawing.Color> c in colors)
        {
            SetLED(c.Key, c.Value, brightness);
        }
    }

    public void SetLED(int ledId, int rgb)
    {
        SetLED(ledId, rgb, 1);
    }
    public void SetLED(int ledId, int rgb, double brightness)
    {
        System.Drawing.Color c = GetColorFromRGB(rgb);
        c = System.Drawing.Color.FromArgb((int)Math.Round(c.R * brightness), (int)Math.Round(c.G * brightness), (int)Math.Round(c.B * brightness));
        colors[ledId] = c;
        GetResponsibleController(ledId)?.SetLED(GetAdjustedLEDPosition(ledId), rgb, brightness);
    }
    
    public void SetLED(int ledId, System.Drawing.Color color, double brightness)
    {
        System.Drawing.Color c = System.Drawing.Color.FromArgb((int)Math.Round(color.R * brightness), (int)Math.Round(color.G * brightness), (int)Math.Round(color.B * brightness));
        colors[ledId] = c;
        GetResponsibleController(ledId)?.SetLED(GetAdjustedLEDPosition(ledId), color, brightness);
    }

    public System.Drawing.Color GetColorFromRGB(int rgb)
    {
        return System.Drawing.Color.FromArgb((rgb >> 16) & 0xff, (rgb >> 8) & 0xff, (rgb >> 0) & 0xff);
    }

    public void SetAllLED(int rgb)
    {
        System.Drawing.Color c = GetColorFromRGB(rgb);
        for(int i = 0; i < LEDCount; i++)
        {
            colors[i] = c;
        }
        foreach (StripWrapper stripWrapper in strips)
        {
            stripWrapper.SetAllLED(rgb);
        }
    }

    public void Render()
    {
        displayedColors = new Dictionary<int, System.Drawing.Color>(colors);
        foreach (StripWrapper stripWrapper in strips)
        {
            stripWrapper.Render();
        }
    }

    private StripWrapper? GetResponsibleController(int i)
    {
        int stripIndex;
        if (!ledToStrip.TryGetValue(i, out stripIndex))
        {
            Logger.Log("LED id " + i + " not found.");
            return null;
        }
        return strips[stripIndex];
    }
    
    int GetAdjustedLEDPosition(int i)
    {
        return i - strips[ledToStrip[i]].LEDStartIndex;
    }

    public void SetLEDBrightness(int i, double brightness)
    {
        SetLED(i, colors[i], brightness);
    }

    public long lastRender = 0;
    public void RenderOncePerFrame(long currentFrame)
    {
        if (lastRender == currentFrame) return;
        lastRender = currentFrame;
        Render();
    }
}

public class StripRepresentation
{
    public Pin pin;
    public int ledCount = 0;
    public int ledStart = 0;
    public bool isVirtual = false;
    public bool reversed = false;
}