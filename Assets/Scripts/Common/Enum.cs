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
        Hero = 1,
        Enemy = 2,
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

    //ָ������
    public enum InstructType
    {
        None = 0,
        Check = 1, //�鿴
        Move = 2, //�ƶ�
        Attack = 3, //����
        Cancel = 4, //ȡ��
        Idle = 5, //����
    }

    public enum EventType
    {
        HUDInstruct_Move_Event = 0,
        HUDInstruct_Attack_Event = 1,
        HUDInstruct_Cancel_Event = 2,
        HUDInstruct_Check_Event = 3,
        HUDInstruct_Select_Event = 4,
        Hero_MoveEnd_Event = 5,
    }
}
