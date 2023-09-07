using System.Runtime.CompilerServices;
using System.Text.Json;
using ComputerUtils.Logging;
using LEDForPi.Strips;

namespace LEDForPi.RBExtras;

public class RBSongPlayer
{
    public static int currentSongId = 0;
    public static MapInfo info = new();
    
    public static bool useGame = true;
    public static RBReplay replay = null;
    
    public static string songId = "";

    public static RBStripController mostRecentStripController;
    
    

    public static void Stop()
    {
        currentSongId++;
    }
}

public class RBSongPlayerConfig
{
    public static RBSongPlayerConfig instance = new RBSongPlayerConfig();

    public static bool flipped
    {
        get => instance._flipped;
        set => instance._flipped = value;
    }
    public static bool enableColorChanges
    {
        get => instance._enableColorChanges;
        set => instance._enableColorChanges = value;
    }
    public static bool enableShakes
    {
        get => instance._enableShakes;
        set => instance._enableShakes = value;
    }
    public static bool enableFlashes
    {
        get => instance._enableFlashes;
        set => instance._enableFlashes = value;
    }
    public static bool enableShip
    {
        get => instance._enableShip;
        set => instance._enableShip = value;
    }
    public static bool enableCubes
    {
        get => instance._enableCubes;
        set => instance._enableCubes = value;
    }
    public static bool enableLaser 
    {
        get => instance._enableLaser;
        set => instance._enableLaser = value;
    }
    public static bool flipShakeDirectionOnColorChange
    {
        get => instance._flipShakeDirectionOnColorChange;
        set => instance._flipShakeDirectionOnColorChange = value;
    }
    public static int playfieldSize
    {
        get => (int)instance._playfieldSize;
        set => instance._playfieldSize = value;
    }
    public static int playfieldStartLEDIndex
    {
        get => (int)instance._playfieldStartLEDIndex;
        set => instance._playfieldStartLEDIndex = value;
    }
    public static bool enableBGColor
    {
        get => instance._enableBGColor;
        set => instance._enableBGColor = value;
    }
    public static double flashTimeCubeShipMs
    {
        get => instance._flashTimeCubeShipMs;
        set => instance._flashTimeCubeShipMs = value;
    }
    public static int waveSpeedMultiplier
    {
        get => (int)instance._waveSpeedMultiplier;
        set => instance._waveSpeedMultiplier = value;
    }
    public static bool brightenShipOnShoot
    {
        get => instance._brightenShipOnShoot;
        set => instance._brightenShipOnShoot = value;
    }
    public bool _flipped { get; set; } = false;
    public bool _enableColorChanges { get; set; } = true;
    public bool _enableShakes { get; set; } = true;
    public bool _enableFlashes { get; set; } = true;
    public bool _enableShip { get; set; } = true;
    public bool _enableCubes { get; set; } = true;
    public bool _enableLaser { get; set; } = true;
    public bool _flipShakeDirectionOnColorChange { get; set; } = true;
    public bool _enableBGColor { get; set; } = true;
    public bool _brightenShipOnShoot { get; set; } = true;
    public double _flashTimeCubeShipMs { get; set; } = .05;
    public double _waveSpeedMultiplier { get; set; } = 20;
    public double _playfieldSize { get; set; } = -1;
    public double _playfieldStartLEDIndex { get; set; } = 0;
    
    public static void Save()
    {
        File.WriteAllText("config.json", JsonSerializer.Serialize(instance));
    }

    public static void Load()
    {
        if (!File.Exists("config.json")) Save();
        instance = JsonSerializer.Deserialize<RBSongPlayerConfig>(File.ReadAllText("config.json"));
    }
}