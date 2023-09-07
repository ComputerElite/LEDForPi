using System;

namespace LEDForPi.RBExtras;

[Serializable]
public class TargetData
{
    public float time { get; set; } = 0.0f; // time of highest point in jump
    public float location { get; set; } = 0.0f; // -1.0 to 1.0
    public float height { get; set; } = 3.0f; // Height of the cube
    public float shootTime { get; set; } = 0.0f; // -1.0 to 1.0 within lifetime
    public float lifetime { get; set; } = 2.0f; // Amount of time the target is on screen. Shoot time will be at half lifetime
    public float size { get; set; } = 0.5f; // size of target
    public int index { get; set; } = 0; // received by server, index of target in map
	[NonSerialized]
	public int gameData = 0; // auto assigned
	public int power { get; set; } = 2; // for animation
    [NonSerialized]
    public UsageTargetType usageType = UsageTargetType.GAME; // auto assigned
    [NonSerialized]
    public float markTime = 0.0f; // auto set, song time on which to mark (zuckerberg) the cube as next to be shot
    [NonSerialized]
    public float unMarkTime = 0.0f; // auto set, song time on which to unmark the cube as next to be shot
	[NonSerialized]
	public bool isAffectedByDrive = false; // auto set, if drive mode can be present on shoot time
	[NonSerialized]
	public int affectedDriveIndex = 0; // auto set, drive mode index
	[NonSerialized]
	public bool isLastDriveCube = false; // auto set, if cube is last drive cube

	public TargetType type { get; set; } = TargetType.NORMAL;

    public TargetData(TargetData toCopy)
    {
        this.time = toCopy.time;
        this.location = toCopy.location;
        this.height = toCopy.height;
        this.shootTime = toCopy.shootTime;
        this.lifetime = toCopy.lifetime;
        this.size = toCopy.size;
        this.power = toCopy.power;
        this.type = toCopy.type;
        this.index = toCopy.index;
    }
	
    public TargetData() { }
	
	public static bool operator ==(TargetData a, TargetData b)
    {
        return IsSame(a, b);
    }

	public static bool IsSame(TargetData a, TargetData b)
	{
        if (a.type != b.type) return false;
		if (a.time != b.time) return false;
		if (a.location != b.location) return false;
		if (a.height != b.height) return false;
		if (a.shootTime != b.shootTime) return false;
		if (a.lifetime != b.lifetime) return false;
		if (a.size != b.size) return false;
		if (a.power != b.power) return false;
        return true;
	}

	public static bool operator !=(TargetData a, TargetData b)
	{
		return !IsSame(a, b);
	}
}

public enum TargetType
{
	NORMAL = 0,
	DECORATIVE = 1, // unused now and not supported by the game anymore
	//	Dear decorative cube type,
	//		You served me well during the early times of the game and have truly received a place in my memories
	//		I remember you as the only target type that was able to add action to the game in the early games
	//		Since you have slowly been replaced by newer, more exciting, less confusion types.
	//		I liked you but sadly you didn't align with the design goal
	//
	//		May you live a fulfilling life on GitHub and YouTube servers
	//	- ComputerElite
	MANUAL = 2,
	BUTTON = 3,
	EXTRA = 4,
	DEADLY = 5,
	FLASH = 6,
	COLORCHANGE = 7,
	DRIVE = 8,
	BPMCHANGE = 9, // experimental, not anymore you true gigachad. It works perfectly fine in the map editor (P. S. there are issues with moving cubes across BPM Change events afaik)
	SHAKE = 10
}

public enum UsageTargetType
{
    GAME,
    MAPPING,
    OUTLINEWHITE,
    OUTLINEPURPLE,
	DRIVE,
	GAMEOVERRIDE // USING GAME OVERRIDE WILL MAKE TARGETS NOT DISAPPEAR IF PROGRESS < -1
}