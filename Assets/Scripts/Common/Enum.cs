using System;

[Serializable]
public class Enum
{
    //地块类型
    public enum HexagonType
    {
        BeachShore = 0,
        DigSite = 1,
        HellLake = 2,
        Lake = 3,
    }

    //Unity对象标签
    public enum Tag
    {
        Hexagon = 0,
    }

    //UI层级
    public enum UILayer
    {
        PanelLayer = 0,
        PopLayer = 1,
        AlertLayer = 2
    }
}
