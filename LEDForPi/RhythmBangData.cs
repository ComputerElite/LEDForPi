namespace LEDForPi;

public class DataReport
{
    public DataType type { get; set; } = DataType.MAP_DIFFICULTY;
    public float time { get; set; } = 0f;
    public float shipPos { get; set; } = 0f;
    public int targetIndex { get; set; } = 0;
    public MapDifficulty map { get; set; } = new MapDifficulty();
}

public enum DataType
{
    MAP_DIFFICULTY,
    UPDATE,
    TARGET_HIT,
    LASER_SHOT
}