using System;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    [Serializable]
    public struct Pair
    {
        public int id;
        public int value;

        public Pair(int id, int value)
        {
            this.id = id;
            this.value = value;
        }
    }

    [Serializable]
    public class Config
    {
        public int ID;
    }

    //[Serializable]
    //public struct SkillStruct
    //{
    //    public int id;
    //    public int level;
    //}

    //��ɫ��
    [Serializable]
    public class RoleConfig: Config
    {
        public int Job;
        public string Name;
        public string Prefab;
        public Enum.RoleType Type;
        public int CommonSkill;
        public int SpecialSkill;
        public string DialogIcon;
    }

    //�ؿ��
    [Serializable]
    public class HexagonConfig : Config
    {
        public string Prefab;
        public bool Reachable;
        public float Resistance;
    }

    //��ɫ�Ǽ���
    [Serializable]
    public class RoleStarConfig : Config
    {
        public float HP;
        public List<Pair> Attrs;
    }

    ////���ܵȼ���
    //[Serializable]
    //public class SkillLevelConfig : Config
    //{
    //    public AttrStruct Attr;
    //}

    //���ܱ�
    [Serializable]
    public class SkillConfig : Config
    {
        public string Prefab;
        public string Icon;
        public string Name;
        public Enum.RoleType TargetType;
        public Enum.AttrType AttrType;
    }

    //װ����
    [Serializable]
    public class EquipmentConfig : Config
    {
        public int Type;
        public string Icon;
        public string Prefab;
        public string Name;
        public Vector3 Rotation;
        public List<Pair> Attrs;
        public List<Pair> Buffs;
    }

    //װ�����ͱ�
    [Serializable]
    public class EquipmentTypeConfig : Config
    {
        public string Name;
        public int[] Combination;
        public Enum.EquipPlace Place;
        public int Animator;
    }

    //�ؿ��
    [Serializable]
    public class HexagonMapConfig
    {
        public string ID;

        public int configId;

        public Vector3 coor;

        public HexagonMapConfig(string id, int configId, Vector3 coor)
        {
            this.ID = id;
            this.configId = configId;
            this.coor = coor;
        }
    }

    //���Ա�
    [Serializable]
    public class AttrConfig:Config
    {
        public string Name;
    }

    //Buff��
    [Serializable]
    public class BufferConfig:Config
    {
        public Pair Attr;
        public int Duration;
        public string Name;
        public string Desc;
    }

    //��ɫְҵ��
    [Serializable]
    public class RoleJobConfig : Config
    {
        public int[] Equipments;
        public string Name;
        public string Icon;
    }

    //����״̬����
    [Serializable]
    public class AnimatorConfig : Config 
    {
        public string Controller;
    }

    //װ����λ��
    [Serializable]
    public class EquipPlaceConfig : Config
    {
        public string SpinePoint;
    }

    //�Ի����
    [Serializable]
    public class DialogGroupConfig : Config
    {
        public int MaxIndex;
    }

    //�Ի���
    [Serializable]
    public class DialogConfig : Config
    {
        public string Context;
        public int Role;
    }

    //�ؿ���
    [Serializable]
    public class LevelConfig : Config 
    {
        public string Name;
        public Vector2 UIPos;
        public string Desc;
        public int LastLevel;
        public Enum.LevelType Type;
        public string Map;
    }
}
