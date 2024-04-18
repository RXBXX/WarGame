using System;

[Serializable]
public class Enum
{
    //�ؿ�����
    public enum HexagonType
    {
        BeachShore = 0,
        DigSite = 1,
        HellLake = 2,
        Lake = 3,
    }

    //Unity�����ǩ
    public enum Tag
    {
        Hexagon = 0,
    }

    //UI�㼶
    public enum UILayer
    {
        PanelLayer = 0,
        PopLayer = 1,
        AlertLayer = 2
    }
}
