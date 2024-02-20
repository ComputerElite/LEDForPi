using rpi_ws281x;

namespace LEDForPi.Strips;

public interface IStrip
{
    
    public int GetLEDCount();
    public Dictionary<int, System.Drawing.Color> GetDisplayedColors();

    public void SetBrightness(double brightness);

    public void SetLED(int ledId, int rgb);
    public void SetLED(int ledId, int rgb, double brightness);

    public void SetLED(int ledId, System.Drawing.Color color, double brightness);

    public System.Drawing.Color GetColorFromRGB(int rgb);

    public void SetAllLED(int rgb);

    public void Render();

    public void SetLEDBrightness(int i, double brightness);
    /// <summary>
    /// Only render if the frame hasn't been rendered yet
    /// </summary>
    /// <param name="currentFrame"></param>
    void RenderOncePerFrame(long currentFrame);
}