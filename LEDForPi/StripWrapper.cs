using System.Drawing;
using rpi_ws281x;

namespace LEDForPi;

public class StripWrapper
{
    public Settings settings;
    public WS281x rpi;
    public Controller controller;
    public int LEDCount => controller.LEDCount;
    public byte brightness => controller.Brightness;
    public void Init(int leds, Pin pin = Pin.Gpio18)
    {
        settings = Settings.CreateDefaultSettings();

        //Use 16 LEDs and GPIO Pin 18.
        //Set brightness to maximum (255)
        //Use Unknown as strip type. Then the type will be set in the native assembly.
        controller = settings.AddController(leds, pin, StripType.WS2812_STRIP, ControllerType.Unknown, 255, false);
        rpi = new WS281x(settings);
    }

    public void SetLED(int ledId, System.Drawing.Color color)
    {
        controller.SetLED(ledId, color);
    }

    public void SetLED(int ledId, int r, int g, int b)
    {
        controller.SetLED(ledId, System.Drawing.Color.FromArgb(r, g, b));
    }
    
    public void SetLED(int ledId, int rgb)
    {
        System.Drawing.Color c = GetColorFromRGB(rgb);
        controller.SetLED(ledId, c);
    }
    public void SetLED(int ledId, int rgb, double brightness)
    {
        System.Drawing.Color c = GetColorFromRGB(rgb);
        c = System.Drawing.Color.FromArgb((int)Math.Round(c.R * brightness), (int)Math.Round(c.G * brightness), (int)Math.Round(c.B * brightness));
        controller.SetLED(ledId, c);
    }

    public System.Drawing.Color GetColorFromRGB(int rgb)
    {
        return System.Drawing.Color.FromArgb((rgb >> 16) & 0xff, (rgb >> 8) & 0xff, (rgb >> 0) & 0xff);
    }
    
    public void SetAllLED(System.Drawing.Color color)
    {
        controller.SetAll(color);
    }

    public void SetAllLED(int r, int g, int b)
    {
        controller.SetAll(System.Drawing.Color.FromArgb(r, g, b));
    }
    
    public void SetAllLED(int rgb)
    {
        controller.SetAll(System.Drawing.Color.FromArgb((rgb >> 16) & 0xff, (rgb >> 8) & 0xff, (rgb >> 0) & 0xff));
    }

    public void Render()
    {
        rpi.Render();
    }
}