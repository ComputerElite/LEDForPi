using System.Device.Gpio;

namespace LEDForPi;

public class Button
{
    private GpioPin pin;
    private GpioController g;
    public Button(int pin, GpioController c)
    {
        g = c;
        this.pin = g.OpenPin(pin, PinMode.Input);
    }
    
    public bool pressed => pin.Read() == PinValue.Low;
}