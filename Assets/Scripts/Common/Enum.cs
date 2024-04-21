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
        Attachable = 3, //可攻击
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
