using System;

namespace WarGame
{
    [Serializable]
    public class Enum
    {
        //�ؿ�����
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

        //Unity�����ǩ
        public enum Tag
        {
            Hexagon = 0,
            Hero = 1,
            Enemy = 2,
            Bonfire = 3,
            Ornament = 4,
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

        ////ָ������
        //public enum InstructType
        //{
        //    None = 0,
        //    Check = 1, //�鿴
        //    Move = 2, //�ƶ�
        //    Attack = 3, //����
        //    Cancel = 4, //ȡ��
        //    Idle = 5, //����
        //}

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
        public enum EquipType
        {
            Wand = 1, //����
            Shield = 2, //����
            Sword = 3, //������
            BowArrow = 4, //����
            Harmer = 5, //��
            Axe = 6, //��
            BigWeapon = 7, //˫������
            Spear = 9, //��ǹ
            Cloak = 10, //����
        }

        //װ�������ʽ
        [Serializable]
        public enum EquipPlace
        {
            Right = 1, //����
            Left = 2, //����
            BothHand = 3, //˫�ֹ���
            RightAndLeft = 4, //������
            Back = 5, //��
        }

        [Serializable]
        public enum Skill
        {
            None = 0,
            FierceAttack = 10001, //�͹�
            SingleHeal = 10002, //��������
            AtrikeAndRelocate = 10003, //������
            ChainAttack = 10004, //��������
            Inspire = 10005, //����
            Stealth = 10006,  //����
            CriticalHit = 10007, //����
            Clone = 10008, //����
            Dizzy = 10009, //ѣ��
            ExtraTurn = 10010, //����һ���ж�
            MassPhyShield = 10011, //Ⱥ��������
            Charm = 10012, //�Ȼ�
            Roulette = 10013, //���̶�
            LifeDrain = 10014, //��Ѫ
            MassHeal = 10015, //ȫ������
            SinglePhyShield = 10016, //���������׶�
            RageReduction = 10017, //��ŭ
            MassMagShield = 10018, //Ⱥ��ħ������
            SingleMagShield = 10019, //����ħ������
        }

        //��������
        [Serializable]
        public enum AttrType
        {
            None = 0,
            HP = 1,
            Rage = 2,
            PhysicalAttack = 3, //������
            MagicAttack = 4, //ħ������
            Cure = 5,//	����
            PhysicalDefense = 6,//	�������
            MagicDefense = 7, // ħ������
            MoveDis = 8,//	�ƶ�����
            AttackDis = 9,//	��������
            DodgeRatio = 10, //������
            PhysicalPenetrateRatio = 11, //����͸
            MagicPenetrateRatio = 12, //ħ����͸
            PhysicalAttackRatio = 13, //��������
            MagicAttackRatio = 14, //ħ������
            RageRecover = 15, //ŭ���ظ�
            HPRecover = 16, //��սѪ���ظ�
            AttackRange = 17, //������Χ
        }

        //��ֵ����
        [Serializable]
        public enum ValueType
        {
            Int = 1,
            Percentage = 2,
        }

        //��������Ŀ��
        [Serializable]
        public enum AttrTargetType
        {
            Self = 0,
            Target = 1
        }

        //�ؿ�����
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
            Opponent = 0, //����
            Friend = 1, //�ѷ�
        }

        //��ǰ�غ�
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

        //����
        [Serializable]
        public enum Element
        {
            None = 0,
            Metal = 1, //��
            Wood = 2, //ľ
            Water = 3, //ˮ
            Fire = 4, //��
            Earth = 5, //��
        }

        //�ؿ��׶�
        [Serializable]
        public enum LevelStage
        {
            None = 0,
            Entered = 1, //����
            Talked = 2, //�Ի�����
            Readyed = 3, //׼�����
            Passed = 4, //�ɹ�ͨ��
            Failed = 5, //ʧ��
        }

        ////��Ϸ����Դ����
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
            Dialog = 1, //�Ի�
            Level = 2, //�ؿ�
            Hero = 3, //����Ӣ��
            Story = 4, //����
            HomeEvent = 5, //���ͼ�¼������ش��ͼ���津��
            Equip = 6, //���װ��
            Items = 7, //��õ���
        }

        public enum RoundType
        {
            OurTurn = 0,
            EnemyTurn = 1,
        }

        //�ؿ�ʤ����������
        public enum LevelWinCondType
        {
            DefeatEnemys = 1, //����ָ������
        }

        public enum Buff
        {
            Blood = 1,
            Posion = 2,
            ReduceRage = 3,
            Cloaking = 4, //����
            Dizzy = 5, //��ѣ
            MassPhyShield = 6, //Ⱥ��������
            MassMagShield = 7, //Ⱥ��ħ������
            SinglePhyShield = 8, //����������
            SingleMagShield = 9, //����ħ������
        }

        // ������buffЧ������
        [Serializable]
        public enum BuffAttrEffectType
        {
            Overlay = 0, //����
            Const = 1, //����
        }

        //buff�仯����
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
            Scene_Load_Progress = 18, //���³������ؽ���
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

        //�����������������Э�飬Ŀǰ��������ָDatasMgr������
        public enum NetProto
        {
            WearEquipS2C = 1, //����װ��
            UnwearEquipS2C = 2, //ж��װ��
        }
    }
}
