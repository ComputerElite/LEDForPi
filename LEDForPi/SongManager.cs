using System.Security.Cryptography;
using System.Text.Json;
using ComputerUtils.FileManaging;
using ComputerUtils.Logging;
using LEDForPi.RBExtras;

namespace LEDForPi;

public class SongManager
{
    public static Dictionary<string, MapInfo> loadedMaps = new Dictionary<string, MapInfo>();

    public static void LoadAllMaps()
    {
        loadedMaps.Clear();
        FileManager.CreateDirectoryIfNotExisting("songs");
        foreach (string songDir in Directory.GetDirectories("songs"))
        {
            MapInfo i = LoadMap(Path.Join(songDir, "map"));
            Logger.Log("Loaded map " + i.song + " - " + i.artist + " (" + CalculateSongId(i) + ")");
            string hash = CalculateSongId(i);
            i.id = hash;
            loadedMaps.Add(hash, i);
        }
    }
    
    public static void ImportZipFile(byte[] zipFile)
    {
        Utils.ExtractZipFile(zipFile, "tmp");
        MapInfo i = LoadMap("tmp");
        FileManager.CreateDirectoryIfNotExisting(GetSongFolderPath(i));
        FileManager.DirectoryCopy("tmp", GetSongMapFolderPath(i), true);
        LoadAllMaps();
        FileManager.DeleteDirectoryIfExisting("tmp");
    }

    public static string GetSongFolderPath(MapInfo i)
    {
        return Path.Join("songs", i.song + " - " + i.artist + "__" + CalculateSongId(i));
    }
    
    public static string GetSongMapFolderPath(MapInfo i)
    {
        return Path.Join(GetSongFolderPath(i), "map");
    }
    public static string GetSongReplayFolderPath(MapInfo i)
    {
        return Path.Join(GetSongFolderPath(i), "replays");
    }

    public static MapInfo LoadMap(string dir)
    {
        if(!dir.EndsWith(Path.DirectorySeparatorChar)) dir += Path.DirectorySeparatorChar;
        MapInfo i = JsonSerializer.Deserialize<MapInfo>(File.ReadAllText(dir + "map.json"));
        
        if (i.songFileName == "") i.songFileName = "song.ogg";
        if (i.coverFileName == "") i.coverFileName = "cover.png";
        i.folder = dir;
        return i;
    }
    
    /// <summary>
    /// Calculates a songs id based on map.json and all difficulties listed in it
    /// </summary>
    /// <param name="info">map to calculate the id of</param>
    /// <returns>the id. (SHA256 in hex)</returns>
    public static string CalculateSongId(MapInfo info)
    {
        // Create a new SHA256 instance
        SHA1 sha1 = SHA1.Create();
        List<byte> bytes = new List<byte>();
        // Add all bytes from map.json followed by the difficulty file bytes in order of appearance in map.json
        bytes.AddRange(File.ReadAllBytes(info.folder + "map.json"));
        foreach (MapDifficultyInfo i in info.difficulties)
        {
            string f = info.folder + i.difficultyFileName;
            if (File.Exists(f))
            {
                bytes.AddRange(File.ReadAllBytes(f));
            }
        }

        // Compute the SHA256 and remove the "-" from the hex string
        string hash = BitConverter.ToString(sha1.ComputeHash(bytes.ToArray())).Replace("-", "");
        return hash;
    }

    public static string ImportReplay(string requestBodyString)
    {
        RBReplay replay = RBReplay.LoadReplay(requestBodyString);
        MapInfo song = GetSongFromLibraryBasedOnId(replay.songId);
        if (song == null) return "Song for this replay not found";
        string replayFileName = replay.diffName + "-" + replay.playerName + " - " + " - " + replay.scoreInfo.score + " - " +
                                replay.scoreInfo.accuracy + ".json";
        FileManager.CreateDirectoryIfNotExisting(GetSongReplayFolderPath(song));
        File.WriteAllText(Path.Join(GetSongReplayFolderPath(song), replayFileName), JsonSerializer.Serialize(replay));
        return "Replay imported for " + song.song + " - " + song.artist + " (" + replay.songId + ")";
    }

    public static MapInfo GetSongFromLibraryBasedOnId(string songId)
    {
        if (!loadedMaps.ContainsKey(songId))
        {
            Logger.Log("Song with id " + songId + " not found");
            return null;
        }
        return loadedMaps[songId];
    }

    public static MapDifficulty LoadDifficulty(MapInfo info, string rDiffName)
    {
        return JsonSerializer.Deserialize<MapDifficulty>(File.ReadAllText(Path.Join(info.folder, rDiffName)));
    }

    public static RBReplay LoadReplay(MapInfo i, string rReplay)
    {
        string replayPath = Path.Join(GetSongReplayFolderPath(i), rReplay);
        if (File.Exists(replayPath))
        {
            Logger.Log("Replay " + replayPath + " loading");
            return RBReplay.LoadReplay(File.ReadAllText(replayPath));
        }
        Logger.Log("Replay " + replayPath + " not found");
        return null;
    }

    public static byte[] GetAudioFile(string songId) => GetAudioFile(GetSongFromLibraryBasedOnId(songId));

    public static byte[] GetAudioFile(MapInfo i)
    {
        if (i == null) return new byte[0];
        return File.ReadAllBytes(GetAudioFilePath(i));
    }

    public static string GetAudioFilePath(MapInfo i)
    {
        return Path.Join(i.folder, i.songFileName);
    }
    public static string GetAudioFilePath(string songId)
    {
        return GetAudioFilePath(GetSongFromLibraryBasedOnId(songId));
    }

    public static List<string> GetReplays(string songId, string diff)
    {
        string replayFolder = GetSongReplayFolderPath(GetSongFromLibraryBasedOnId(songId));
        FileManager.CreateDirectoryIfNotExisting(replayFolder);
        return Directory.GetFiles(replayFolder)
            .Where(x => Path.GetFileName(x).StartsWith(diff + "-")).ToList().ConvertAll(x => Path.GetFileName(x));
    }
}