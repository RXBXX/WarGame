using System;

namespace WarGame
{
    [Serializable]
    public class Enum
    {
        //地块类型
        public enum HexagonType
        {
            BeachShore = 1,
            DigSite = 2,
            HellLake = 3,
            Lake = 4,
            Born = 5,
            Fire1 = 6,
            Fire2 = 7,
            Fire3 = 8,
            Fire4 = 9
        }

        //Unity对象标签
        public enum Tag
        {
            Hexagon = 0,
            Hero = 1,
            Enemy = 2,
            Bonfire = 3,
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
        public enum EquipType
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
        public enum Skill
        {
            Attack = 10001,
            Cure = 10002,
            AttackRetreat = 10003,
            GroupAttack = 10004,
            Inspire = 10005,
        }

        //属性类型
        [Serializable]
        public enum AttrType
        {
            HP = 1,
            Rage = 2,
            PhysicalAttack = 3, //攻击
            MagicAttack = 4,
            Cure = 5,//	治疗
            PhysicalDefense = 6,//	防御
            MagicDefense = 7,
            MoveDis = 8,//	移动距离
            AttackDis = 9,//	攻击距离
            DodgeRatio = 10, //闪避率
            PhysicalPenetrateRatio = 11, //物理穿透
            MagicPenetrateRatio = 12, //魔法穿透
            PhysicalAttackRatio = 13, //物理增伤
            MagicAttackRatio = 14, //魔法增伤
            RageRecover = 15, //怒气回复
        }

        //数值类型
        [Serializable]
        public enum ValueType
        {
            Int = 1,
            Percentage = 2,
        }

        //属性作用目标
        [Serializable]
        public enum AttrTargetType
        {
            Self = 0,
            Target = 1
        }

        //关卡类型
        public enum LevelType
        {
            Main = 1,
            Branch = 2,
        }

        public enum ErrorCode
        {
            Success = 0,
            Error = 1,
        }

        public enum PostProcessingType
        {
            Gray = 0
        }

        [Serializable]
        public enum TargetType
        {
            Opponent = 0, //对手
            Friend = 1, //友方
        }

        //当前回合
        public enum FightTurn
        {
            HeroTurn = 0,
            EnemyTurn = 1,
        }

        public enum Layer
        {
            Default = 0,
            Hero = 6,
            Enemy = 7,
            Gray = 8,
            Display = 9,
        }

        //五行
        [Serializable]
        public enum Element
        {
            Metal = 1, //金
            Wood = 2, //木
            Water = 3, //水
            Fire = 4, //火
            Earth = 5, //土
        }

        //关卡阶段
        [Serializable]
        public enum LevelStage
        {
            None = 0,
            Entered = 1, //进入
            Talked = 2, //对话结束
            Readyed = 3, //准备完成
            Passed = 4, //成功通关
        }

        //游戏内资源类型
        [Serializable]
        public enum SourceType
        {
            Hero = 1,
            Equip = 2,
        }

        public enum RecordMode
        {
            Read = 1,
            Write = 2,
        }

        [Serializable]
        public enum ActionType
        {
            HeroAction = 0,
            EnemyAction = 1,
        }

        public enum EventType
        {
            HUDInstruct_Idle_Event = 0,
            HUDInstruct_Click_Skill = 1,
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
            Fight_AI_Start = 12,
            Map_Open_Event = 13,
            HUDInstruct_Cancel_Skill = 14,
            Hero_Wear_Equip = 17,
            Scene_Load_Progress = 18, //更新场景加载进度
            Fight_Show_HP = 20,
            Fight_Close_HP = 21,
            Role_Attr_Change = 22,
            Hero_Unwear_Equip = 23,
            Fight_Action_Over = 24,
            Fight_AI_Move = 25,
            Fight_AI_MoveStart = 26,
            Fight_AI_MoveEnd = 27,
            Fight_Skill_Over = 28,
            Fight_Battle = 29,
            Fight_Cured_End = 30,
            Fight_Infor_Show = 31,
            Fight_Infor_Hide = 32,
            Fight_AIAction_Over = 33,
            Fight_Skip_Rount = 34,
            Fight_Role_Dispose = 35,
            Fight_Show_HeroGroup = 36,
            Fight_Change_Hero = 37,
            Fight_Start = 38,
            Fight_AI_Over = 39,
            Fight_AIAction_Start = 40,
            Save_Data = 41,
            Hero_Show_Attrs = 42,
            Hero_Show_Talent = 43,
            Fight_Dodge_End = 44,
            Fight_Hide_HeroGroup = 45,
            Fight_Show_RoleInfo = 46,
            Fight_Hide_RoleInfo = 47,
            HeroTalentActiveS2C = 10001,
        }
    }
}
