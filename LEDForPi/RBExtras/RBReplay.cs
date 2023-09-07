using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using LEDForPi;

namespace LEDForPi.RBExtras;

[Serializable]
public enum RBReplayVersion
{
    Unknown = -1,
    V01 = 0, // first public replay version
    V02 = 1, // Added support for miss animations
    V03 = 2, // Adjusted score curve PTB 1.3.0
	V04 = 3, // Adjusted cube indexes making all old replays miss cubes
    V05 = 4, // Added score info
}

[Serializable]
public class RBReplay
{
    public string songId{ get; set; } = "";
    public string diffName { get; set; }= "";
    public string playerName { get; set; }= "";
    public GameplayModifiers gameplayModifiers { get; set; }= new();
    public List<RBReplayFrame> frames { get; set; }= new();
    public List<RBReplayTarget> targets { get; set; }= new();
    public List<RBReplayMissAnimation> missAnimations { get; set; }= new();
    public List<float> shootTimes { get; set; }= new();
    public RBReplayVersion formatVersion { get; set; }= RBReplayVersion.V01;
    public RBReplayScore scoreInfo { get; set; }= new();

    public int GetVersionByte()
    {
        switch (formatVersion)
        {
            case RBReplayVersion.V01:
                return 0x01;
            case RBReplayVersion.V02:
                return 0x02;
            case RBReplayVersion.V03:
                return 0x03;
			case RBReplayVersion.V04:
				return 0x04;
            case RBReplayVersion.V05:
                return 0x05;
			default:
                return 0x00;
        }
    }

    public static RBReplayVersion GetVersion(int b)
    {
        switch(b)
        {
            case 0x01:
                return RBReplayVersion.V01;
            case 0x02:
                return RBReplayVersion.V02;
            case 0x03:
                return RBReplayVersion.V03;
			case 0x04:
				return RBReplayVersion.V04;
            case 0x05:
                return RBReplayVersion.V05;
			default:
                return RBReplayVersion.Unknown;
        }
    }

    public RBReplay() { }

    public void PlayFrame()
    {
        for(int i = 0; i < frames.Count; i++)
        {
            if (RBSongPlayer.mostRecentStripController.elapsedSeconds >= frames[i].t)
            {
                RBSongPlayer.mostRecentStripController.SetShipPos(frames[i].p);
                frames.RemoveAt(i);
                i--;
            } else
            {
                break;
            }
        }

        for (int i = 0; i < shootTimes.Count; i++)
        {
            if (RBSongPlayer.mostRecentStripController.elapsedSeconds >= shootTimes[i])
            {
                RBSongPlayer.mostRecentStripController.LaserShot();
                shootTimes.RemoveAt(i);
                i--;
            } else
            {
                break;
            }
        }
    }

	public void PlayFirstFrame()
	{
		RBSongPlayer.mostRecentStripController.SetShipPos(frames[0].p);
        if (gameplayModifiers.faster)
        {
            RBSongPlayer.mostRecentStripController.SetSpeed(1.25f);
        } else if (gameplayModifiers.slower)
        {
            RBSongPlayer.mostRecentStripController.SetSpeed(.75f);
        } else {
            RBSongPlayer.mostRecentStripController.SetSpeed(1f);
            
        }
	}

    public RBReplayTarget GetCubeData(TargetData data)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].i == data.index) return targets[i];
        }
        return new RBReplayTarget();
    }

    public static RBReplay LoadReplay(string replay)
    {
        return TryMakeReplayCompatible(JsonSerializer.Deserialize<RBReplay>(replay));
    }

	public static RBReplay TryMakeReplayCompatible(RBReplay replay)
    {
		if (replay.formatVersion < RBReplayVersion.V04 && replay.formatVersion > RBReplayVersion.Unknown)
        {
			// Adjust cube indexes from old (all types included) to new (only manual and normal included)
			for(int i = 0; i < replay.targets.Count; i++)
            {
                replay.targets[i].i = i;
            }
        }
        return replay;
	}
}

[Serializable]
public class RBReplayScore
{
    public int score{ get; set; } = 0;
    public string rank { get; set; }= "";
    public bool FC { get; set; }= false;
    public float accuracy { get; set; }= 0f;
    
    public int combo { get; set; }= 0;
    public int maxCombo { get; set; }= 0; // max combo you had during the song
    public int hits { get; set; }= 0; // how many normal and manual cubes you hit
    public int manualCubesHit{ get; set; } = 0; // how many manual cubes you hit
    public int normalCubesHit{ get; set; } = 0; // how many normal cubes you hit
    public int deadlyCubesHit { get; set; }= 0; // how many deadly cubes you hit
    public int misses { get; set; }= 0; // how many normal and manual cubes you missed
    public int manualCubesMissed { get; set; }= 0; // how many manual cubes you missed
    public int normalCubesMissed { get; set; }= 0; // how many normal cubes you missed
    public int rawScore{ get; set; } = 0; // score without multiplier

    public int scoreOfNormalAndManualCubes{ get; set; } = 0;
}

[Serializable]
public class RBReplayFrame
{
    /// <summary>
    /// position x -1 to 1
    /// </summary>
    public float p { get; set; }= 0f; // position
    /// <summary>
    /// position y -1 to 1
    /// </summary>
    public float y { get; set; }= 0f; // position
    /// <summary>
    /// time in song
    /// </summary>
    public float t { get; set; }= 0f; // time
    /// <summary>
    /// health at current frame
    /// </summary>
    public float h { get; set; }= 0f; // health
}

[Serializable]
public class RBReplayMissAnimation
{
    /// <summary>
    /// time the animation should be played
    /// </summary>
    public float t { get; set; }= 0; // time
    /// <summary>
    /// position the animation should be played
    /// </summary>
    public float p { get; set; }= 0f; // position
}

[Serializable]
public class RBReplayTarget
{
    /// <summary>
    /// score the target got (raw)
    /// </summary>
    public float s{ get; set; } = 0; // score (raw)
    /// <summary>
    /// if the target has been hit
    /// </summary>
    public bool h { get; set; }= false; // hit
    /// <summary>
    /// index of the target
    /// </summary>
    public int i{ get; set; } = 0; // index
    /// <summary>
    /// when in progress the target has been hit
    /// </summary>
    public float t { get; set; }= 0f; // hitTime, only on manual cubes
}