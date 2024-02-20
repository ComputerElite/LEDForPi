namespace LEDForPi;

public class ColorUtils
{
    public static int HsvToRgb(double h, double s, double v)
    {
        h %= 360;
        if (s == 0)
        {
            int value = (int)(v * 255);
            return (value << 16) | (value << 8) | value;
        }

        h /= 60.0;
        int i = (int)h;
        double f = h - i;
        double p = v * (1 - s);
        double q = v * (1 - s * f);
        double t = v * (1 - s * (1 - f));

        double r, g, b;
    
        switch (i)
        {
            case 0:
                r = v;
                g = t;
                b = p;
                break;
            case 1:
                r = q;
                g = v;
                b = p;
                break;
            case 2:
                r = p;
                g = v;
                b = t;
                break;
            case 3:
                r = p;
                g = q;
                b = v;
                break;
            case 4:
                r = t;
                g = p;
                b = v;
                break;
            default:
                r = v;
                g = p;
                b = q;
                break;
        }

        int red = (int)(r * 255);
        int green = (int)(g * 255);
        int blue = (int)(b * 255);

        return (red << 16) | (green << 8) | blue;
    }
}