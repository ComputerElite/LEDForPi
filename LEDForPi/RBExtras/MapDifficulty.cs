using System;
using System.Collections.Generic;
using LEDForPi.RBExtras;

[Serializable]
public class MapDifficulty
{
    public List<TargetData> targets { get; set; }= new();

    public MapDifficulty()
    {

    }

    public MapDifficulty(MapDifficulty m)
    {
        targets = new List<TargetData>(m.targets);
    }
}