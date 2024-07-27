using System;

namespace WarGame
{
    [Serializable]
    public class Enum
    {
        //地块类型
        public enum HexagonType
        {
            Hex1 = 1,
            Hex2 = 2,
            Hex3 = 3,
            Hex4 = 4,
            Hex5 = 5,
            Hex6 = 6,
            Hex7 = 7,
            Hex8 = 8,
            Hex9 = 9,
            Hex10 = 10,
            Hex11 = 11,
            Hex12 = 12,
            Hex13 = 13,
            Hex14 = 14,
            Hex15 = 15,
            Hex16 = 16,
            Hex17 = 17,
            Hex18 = 18,
            Hex19 = 19,
            Hex20 = 20,
            Hex21 = 21,
            Hex22 = 22,
            Hex23 = 23,
            Hex24 = 24,
            Hex25 = 25,
            Hex26 = 26,
            Hex27 = 27,
            Hex28 = 28,
            Hex29 = 29,
            Hex30 = 30,
            Hex31 = 31,
            Hex32 = 32,
            Hex33 = 33,
            Hex34 = 34,
            Hex35 = 35,
            Hex36 = 36,
        }

        //Unity对象标签
        public enum Tag
        {
            Hexagon = 0,
            Hero = 1,
            Enemy = 2,
            Bonfire = 3,
            Ornament = 4,
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

        ////指令类型
        //public enum InstructType
        //{
        //    None = 0,
        //    Check = 1, //查看
        //    Move = 2, //移动
        //    Attack = 3, //攻击
        //    Cancel = 4, //取消
        //    Idle = 5, //待机
        //}

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
            Sword = 3, //剑、刀
            BowArrow = 4, //弓箭
            Harmer = 5, //锤
            Axe = 6, //斧
            BigWeapon = 7, //双手武器
            Spear = 9, //长枪
            Cloak = 10, //披风
        }

        //装备佩戴方式
        [Serializable]
        public enum EquipPlace
        {
            Right = 1, //右手
            Left = 2, //左手
            BothHand = 3, //双手公用
            RightAndLeft = 4, //左右手
            Back = 5, //后背
        }

        [Serializable]
        public enum Skill
        {
            None = 0,
            FierceAttack = 10001, //猛攻
            SingleHeal = 10002, //单体治疗
            AtrikeAndRelocate = 10003, //攻击后撤
            ChainAttack = 10004, //锁链攻击
            Inspire = 10005, //鼓舞
            Stealth = 10006,  //隐身
            CriticalHit = 10007, //暴击
            Clone = 10008, //分身
            Dizzy = 10009, //眩晕
            ExtraTurn = 10010, //增加一次行动
            MassPhyShield = 10011, //群体物理护盾
            Charm = 10012, //魅惑
            Roulette = 10013, //轮盘赌
            LifeDrain = 10014, //吸血
            MassHeal = 10015, //全体治疗
            SinglePhyShield = 10016, //单体物理套盾
            RageReduction = 10017, //减怒
            MassMagShield = 10018, //群体魔法护盾
            SingleMagShield = 10019, //单体魔法护盾
        }

        //属性类型
        [Serializable]
        public enum AttrType
        {
            None = 0,
            HP = 1,
            Rage = 2,
            PhysicalAttack = 3, //物理攻击
            MagicAttack = 4, //魔法攻击
            Cure = 5,//	治疗
            PhysicalDefense = 6,//	物理防御
            MagicDefense = 7, // 魔法防御
            MoveDis = 8,//	移动距离
            AttackDis = 9,//	攻击距离
            DodgeRatio = 10, //闪避率
            PhysicalPenetrateRatio = 11, //物理穿透
            MagicPenetrateRatio = 12, //魔法穿透
            PhysicalAttackRatio = 13, //物理增伤
            MagicAttackRatio = 14, //魔法增伤
            RageRecover = 15, //怒气回复
            HPRecover = 16, //脱战血量回复
            AttackRange = 17, //攻击范围
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
            Gray = 0,
            Fog = 1,
            Palette = 2,
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
            None = 0,
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
            Failed = 5, //失败
        }

        ////游戏内资源类型
        //[Serializable]
        //public enum SourceType
        //{
        //    Hero = 1,
        //    Equip = 2,
        //    Item = 3,
        //}

        [Serializable]
        public enum ItemType
        {
            TalentRes = 1,
            LevelRes = 2,
            EquipRes = 3,
        }

        public enum RecordMode
        {
            Read = 0,
            Write = 1,
        }

        [Serializable]
        public enum ActionType
        {
            ReadyAction = 0,
            HeroAction = 1,
            EnemyAction = 2,
        }

        [Serializable]
        public enum EventType
        {
            Dialog = 1, //对话
            Level = 2, //关卡
            Hero = 3, //激活英雄
            Story = 4, //故事
            HomeEvent = 5, //大地图事件，返回大地图界面触发
            Equip = 6, //获得装备
            Items = 7, //获得道具
        }

        public enum RoundType
        {
            OurTurn = 0,
            EnemyTurn = 1,
        }

        //关卡胜利条件类型
        public enum LevelWinCondType
        {
            DefeatEnemys = 1, //击败指定敌人
        }

        public enum Buff
        {
            Blood = 1,
            Posion = 2,
            ReduceRage = 3,
            Cloaking = 4, //隐身
            Dizzy = 5, //晕眩
            MassPhyShield = 6, //群体物理护盾
            MassMagShield = 7, //群体魔法护盾
            SinglePhyShield = 8, //单体物理护盾
            SingleMagShield = 9, //单体魔法护盾
        }

        // 属性类buff效果类型
        [Serializable]
        public enum BuffAttrEffectType
        {
            Overlay = 0, //叠加
            Const = 1, //常量
        }

        //buff变化类型
        public enum BuffUpdate
        {
            None = 0,
            Add = 1,
            Delete = 2,
        }

        public enum SettingsType
        {
            Sound = 0,
            Language = 1,
            Fight = 2,
        }

        public enum SoundType
        {
            Music = 0,
            Audio = 1,
        }

        public enum FightType
        {
            SkipBattleArena = 0,
        }

        public enum LanguageType
        {
            Text = 0
        }

        public enum AudioListenerType
        {
            TwoD = 0,
            ThreeD = 1,
        }

        public enum Event
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
            HUDInstruct_Attack = 15,
            WearEquipS2C = 17,
            Scene_Load_Progress = 18, //更新场景加载进度
            Fight_Show_HP = 20,
            Fight_Close_HP = 21,
            Role_Attr_Change = 22,
            UnwearEquipS2C = 23,
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
            Fight_Cure_End = 48,
            Role_Create_Success = 49,
            Fight_Drops = 50,
            Language_Changed = 51,
            HeroTalentActiveS2C = 10001,
            HeroLevelUpS2C = 10002,
            ActiveLevelS2C = 10003,
            BuyEquipS2C = 10004,
            SetLanguageS2C = 10005,
        }

        //负责与服务器交互的协议，目前服务器是指DatasMgr管理类
        public enum NetProto
        {
            WearEquipS2C = 1, //穿戴装备
            UnwearEquipS2C = 2, //卸载装备
        }
    }
}
