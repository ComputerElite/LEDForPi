namespace LEDForPi;

public class Animation
{
    public const string RainbowStatic = "4";
    public const string RainbowFade = "3";
    public const string RainbowLeftRightBounceMiddle = "0";
    public const string RainbowLeftRightBounce = "1";
    public const string RainbowLeftRight = "2";
    public const string Blink = "5";
    public const string Static = "6";
    public const string LeftTurnSignal = "7";
    public const string RightTurnSignal = "8";

    public static Dictionary<string, string> GetAnimations()
    {
        Dictionary<string, string> animationDictionary = new Dictionary<string, string>();

        // Get all public static fields of the Animation class
        var fields = typeof(Animation).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(string))
            {
                string fieldName = field.Name;
                string fieldValue = (string)field.GetValue(null); // null argument for static fields

                animationDictionary.Add(fieldValue, fieldName);
            }
        }

        return animationDictionary;
    }
}