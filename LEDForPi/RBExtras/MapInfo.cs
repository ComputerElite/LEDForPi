namespace LEDForPi.RBExtras;

[Serializable]
public class MapInfo
{
    public string id { get; set; } = "";
    public string mapper { get; set; } = "";
    public string song { get; set; } = "";
    public string artist { get; set; } = "";
    public string songFileName { get; set; } = "";
    /// <summary>
    /// The songs folder name, should be unique for custom songs
    /// </summary>
    public string folderName { get; set; } = "";
    public string coverFileName { get; set; } = "";
    public string tmpFolder { get; set; } = ""; // auto set

    /// <summary>
    /// The complete path to the song. Absolute if custom songs, relative if built in
    /// </summary>
    public string folder { get; set; } = ""; // auto set
    public int intensity { get; set; } = 0; // difficulty of the song
    public float bpm { get; set; } = 120.0f;
    public int length { get; set; } = 0;
    public float offset { get; set; } = 0f;
    public bool isInResources { get; set; } = true;
    public float previewAudioStart { get; set; } = 0.0f;
    public float previewAudioLength { get; set; } = 30.0f;
    public List<Color> colors { get; set; } = new()
    {
        new(.76f, .31f, .53f),
        new(.12f, 0f, .78f),
        new(.78f, 0f, .12f)
    };
    public List<Color> bgColors { get; set; } = new()
    {
        new(.11f, 0f, .11f),
        new(.02f, 0f, .13f),
        new(.13f, 0f, .02f)
    };
    public List<MapDifficultyInfo> difficulties { get; set; } = new();
    public DateTime lastFolderChangeTime { get; set; } = DateTime.MinValue;
    public float beatsPerMeasure { get; set; } = 4f;
    public bool isSortType { get; set; } = false;

    public MapInfo()
    {
        
    }
}

public class Color
{
    public float r { get; set; } = 0f;
    public float g { get; set; } = 0f;
    public float b { get; set; } = 0f;

    public string hex
    {
        get
        {
            return ToInt().ToString("X6");
        }
    }
    
    public Color()
    {
        
    }
    
    public Color(float r, float g, float b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
    
    // Multiplication
    public static Color operator *(Color a, float b)
    {
        return new Color(a.r * b, a.g * b, a.b * b);
    }

    public static Color Lerp(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0, 1);
        return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t);
    }

    public int ToInt()
    {
        return ((int)(r * 255) << 16) + ((int)(g * 255) << 8) + (int)(b * 255);
    }

    public override string ToString()
    {
        return r + " " + g + " " + b;
    }
}

[Serializable]
public class MapDifficultyInfo
{
    public string difficultyFileName { get; set; } = "";
    public string difficultyName { get; set; } = "";

    public MapDifficultyInfo()
    {

    }

    public MapDifficultyInfo(MapDifficultyInfo m)
    {
        difficultyFileName = m.difficultyFileName;
        difficultyName = m.difficultyName;
    }
}