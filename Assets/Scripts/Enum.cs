using System;

[Serializable]
public class Enum
{
    [Serializable]
    public enum HexagonType
    {
        BeachShore = 0,
        DigSite = 1,
        HellLake = 2,
        Lake = 3,
    }

    public enum Tag
    {
        Hexagon = 0,
    }
}
