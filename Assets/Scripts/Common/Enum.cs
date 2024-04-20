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
        HUDLayer = 0,
        PanelLayer = 1,
        PopLayer = 2,
        AlertLayer = 3,
    }

    //地块标记类型
    public enum MarkType
    { 
        None = 0,
        Selected = 1, //被选中
        Walkable = 2, //可抵达
        Attachable = 3, //可攻击
        Target = 4, //攻击目标
    }
}
