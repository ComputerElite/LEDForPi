using System.IO.Compression;
using ComputerUtils.FileManaging;
using LEDForPi.RBExtras;
using LEDForPi.Strips;

namespace LEDForPi;

public class Utils
{

    public static int LocationToLEDIndex(float location, VirtualStrip stripWrapper)
    {
        if (RBSongPlayerConfig.flipped)
        {
            return (int)((location + 1) * (RBSongPlayerConfig.playfieldSize - 1) / 2f) + RBSongPlayerConfig.playfieldStartLEDIndex;
        }

        return (stripWrapper.LEDCount - 1) - (int)((location + 1) * (RBSongPlayerConfig.playfieldSize - 1) / 2f) - RBSongPlayerConfig.playfieldStartLEDIndex;
    }
    
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static double PerlinNoise(float x)
    {
        return Math.Sin(x + 2) * Math.Sin(2 * x + 1) * -Math.Sin(2.5f * x + 1);
    }
    
    public static void ExtractZipFile(byte[] zipFileBytes, string outputDirectory)
    {
        FileManager.DeleteDirectoryIfExisting(outputDirectory);
        using (MemoryStream memoryStream = new MemoryStream(zipFileBytes))
        {
            using (ZipArchive zipArchive = new ZipArchive(memoryStream))
            {
                // Create the output directory if it doesn't exist
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // Extract each entry in the zip file to the output directory
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    string entryPath = Path.Combine(outputDirectory, entry.FullName);

                    // Ensure the target directory for the entry exists
                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                    // Extract the entry to the target directory
                    entry.ExtractToFile(entryPath, true);
                }
            }
        }
    }

}