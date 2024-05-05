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
        Attackable = 3, //�ɹ���
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

    /// <summary>
    /// ��ɫ��Ϊ״̬
    /// </summary>
    public enum RoleState
    {
        Locked = 0,
        Waiting = 1,
        Moving = 2,
        WaitingOrder = 3,
        ReturnMoving = 4,
        WatingTarget = 5, //Ѱ��Ŀ��
        Attacking = 6,  //������
        Over = 7,
    }

    public enum RoleType
    {
        None = 0,
        Hero = 1,
        Enemy = 2,
    }

    /// <summary>
    /// ��ɫ����״̬
    /// </summary>
    public enum RoleAnimState
    {
        Start = 0,
        Take = 1,
        Loss = 2,
        End = 3
    }

    [Serializable]
    public enum EquipmentType
    {
        Wand = 1, //����
        Shield = 2, //����
        Sword = 3, //��
        Knife = 4, //��
        Bow = 5, //��
        Harmer = 6, //��
        Axe = 7, //��
        BigWeapon = 8, //˫������
        Arrow = 9, //��
    }

    //װ�������ʽ
    [Serializable]
    public enum EquipPlace
    {
        Right = 1, //����
        Left = 2, //����
        BothHand = 3, //˫��
    }

    [Serializable]
    public enum SkillType
    {
        Common = 0,
        Special = 1,
    }

    //��������
    public enum AttrType
    {
        Attack = 1, //����
        Cure = 2,//	����
        Defense = 3,//	����
        Move = 4,//	�ƶ�����
        AttackDis = 5,//	��������
        Blood = 6,//	��Ѫ
    }

    public enum EventType
    {
        HUDInstruct_Idle_Event = 0,
        HUDInstruct_Attack_Event = 1,
        HUDInstruct_Cancel_Event = 2,
        HUDInstruct_Check_Event = 3,
        HUDInstruct_Select_Event = 4,
        Role_MoveEnd_Event = 5,
        Fight_Event = 6,
        Fight_RoundChange_Event = 7,
        Fight_RoundOver_Event = 8,
        Fight_Attack_End = 9,
        Fight_Attacked_End = 10,
        Fight_Dead_End = 11,
        Fight_Attack = 12,
        Fight_AI_Start = 13,
        Map_Open_Event = 14,
        Fight_Cancel = 15,
        Hero_Open_Equip = 16,
        Hero_Open_Skill = 17,
        Hero_Wear_Equip = 18,
    }
}
