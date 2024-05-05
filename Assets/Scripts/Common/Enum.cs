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
        Hero = 1,
        Enemy = 2,
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
        Attackable = 3, //可攻击
        Target = 4, //攻击目标
    }

    //指令类型
    public enum InstructType
    {
        None = 0,
        Check = 1, //查看
        Move = 2, //移动
        Attack = 3, //攻击
        Cancel = 4, //取消
        Idle = 5, //待机
    }

    /// <summary>
    /// 角色行为状态
    /// </summary>
    public enum RoleState
    {
        Locked = 0,
        Waiting = 1,
        Moving = 2,
        WaitingOrder = 3,
        ReturnMoving = 4,
        WatingTarget = 5, //寻找目标
        Attacking = 6,  //攻击中
        Over = 7,
    }

    public enum RoleType
    {
        None = 0,
        Hero = 1,
        Enemy = 2,
    }

    /// <summary>
    /// 角色动作状态
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
        Wand = 1, //法杖
        Shield = 2, //盾牌
        Sword = 3, //剑
        Knife = 4, //刀
        Bow = 5, //弓
        Harmer = 6, //锤
        Axe = 7, //斧
        BigWeapon = 8, //双手武器
        Arrow = 9, //箭
    }

    //装备佩戴方式
    [Serializable]
    public enum EquipPlace
    {
        Right = 1, //右手
        Left = 2, //左手
        BothHand = 3, //双手
    }

    [Serializable]
    public enum SkillType
    {
        Common = 0,
        Special = 1,
    }

    //属性类型
    public enum AttrType
    {
        Attack = 1, //攻击
        Cure = 2,//	治疗
        Defense = 3,//	防御
        Move = 4,//	移动距离
        AttackDis = 5,//	攻击距离
        Blood = 6,//	流血
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
