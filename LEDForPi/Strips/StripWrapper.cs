using System.Drawing;
using rpi_ws281x;

namespace LEDForPi.Strips;

public class StripWrapper : IStrip
{
    public Settings settings;
    public WS281x rpi;
    public Controller controller;

    public int LEDCount
    {
        get
        {
            {
                if (isVirtual) return _ledCount;
                return controller.LEDCount;
            }
        }
    }

    public int _ledCount = 0;
    public int LEDStartIndex = 0;
    public Dictionary<int, System.Drawing.Color> colors = new();
    public Dictionary<int, System.Drawing.Color> displayedColors = new();
    public bool isVirtual = false;

    public int GetLEDCount()
    {
        return LEDCount;
    }

    public Dictionary<int, System.Drawing.Color> GetDisplayedColors()
    {
        return displayedColors;
    }

    public void Init(int leds, Pin pin = Pin.Gpio18)
    {
        _ledCount = leds;
        if (isVirtual) return;
        settings = Settings.CreateDefaultSettings();

        //Use 16 LEDs and GPIO Pin 18.
        //Set brightness to maximum (255)
        //Use Unknown as strip type. Then the type will be set in the native assembly.
        controller = settings.AddController(leds, pin, StripType.WS2812_STRIP, ControllerType.Unknown, 255, false);
        rpi = new WS281x(settings);
    }
    
    public void SetBrightness(double brightness)
    {
        if (isVirtual) return;
        foreach (KeyValuePair<int,System.Drawing.Color> c in colors)
        {
            SetLED(c.Key, c.Value, brightness);
        }
    }

    public void SetLED(int ledId, int rgb)
    {
        System.Drawing.Color c = GetColorFromRGB(rgb);
        colors[ledId] = c;
        if(!isVirtual) controller.SetLED(ledId, c);
    }
    public void SetLED(int ledId, int rgb, double brightness)
    {
        System.Drawing.Color c = GetColorFromRGB(rgb);
        c = System.Drawing.Color.FromArgb((int)Math.Round(c.R * brightness), (int)Math.Round(c.G * brightness), (int)Math.Round(c.B * brightness));
        colors[ledId] = c;
        if(!isVirtual) controller.SetLED(ledId, c);
    }
    
    public void SetLED(int ledId, System.Drawing.Color color, double brightness)
    {
        System.Drawing.Color c = System.Drawing.Color.FromArgb((int)Math.Round(color.R * brightness), (int)Math.Round(color.G * brightness), (int)Math.Round(color.B * brightness));
        colors[ledId] = c;
        if(!isVirtual) controller.SetLED(ledId, c);
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
        if(!isVirtual) controller.SetAll(c);
    }

    public void Render()
    {
        displayedColors = new Dictionary<int, System.Drawing.Color>(colors);
        if(!isVirtual) rpi.Render();
    }

    public void SetLEDBrightness(int i, double brightness)
    {
        SetLED(i, colors[i], brightness);
    }
}