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
        HUDLayer = 0,
        PanelLayer = 1,
        PopLayer = 2,
        AlertLayer = 3,
    }

    //�ؿ�������
    public enum MarkType
    { 
        None = 0,
        Selected = 1, //��ѡ��
        Walkable = 2, //�ɵִ�
        Attachable = 3, //�ɹ���
        Target = 4, //����Ŀ��
    }
}
