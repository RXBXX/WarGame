using System;

namespace WarGame
{
    [Serializable]
    public class Enum
    {
        //�ؿ�����
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

        //Unity�����ǩ
        public enum Tag
        {
            Hexagon = 0,
            Hero = 1,
            Enemy = 2,
            Bonfire = 3,
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
        public enum EquipType
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
        public enum Skill
        {
            Attack = 10001,
            Cure = 10002,
            AttackRetreat = 10003,
            GroupAttack = 10004,
            Inspire = 10005,
        }

        //��������
        [Serializable]
        public enum AttrType
        {
            HP = 1,
            Rage = 2,
            PhysicalAttack = 3, //����
            MagicAttack = 4,
            Cure = 5,//	����
            PhysicalDefense = 6,//	����
            MagicDefense = 7,
            MoveDis = 8,//	�ƶ�����
            AttackDis = 9,//	��������
            DodgeRatio = 10, //������
            PhysicalPenetrateRatio = 11, //����͸
            MagicPenetrateRatio = 12, //ħ����͸
            PhysicalAttackRatio = 13, //��������
            MagicAttackRatio = 14, //ħ������
            RageRecover = 15, //ŭ���ظ�
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
            Gray = 0
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
        }

        //��Ϸ����Դ����
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
            Scene_Load_Progress = 18, //���³������ؽ���
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
