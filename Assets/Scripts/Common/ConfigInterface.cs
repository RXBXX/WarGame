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

    //角色表
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

    //地块表
    [Serializable]
    public class HexagonConfig : Config
    {
        public string Prefab;
        public bool Reachable;
        public float Resistance;
    }

    //角色星级表
    [Serializable]
    public class RoleStarConfig : Config
    {
        public float HP;
        public List<Pair> Attrs;
    }

    ////技能等级表
    //[Serializable]
    //public class SkillLevelConfig : Config
    //{
    //    public AttrStruct Attr;
    //}

    //技能表
    [Serializable]
    public class SkillConfig : Config
    {
        public string Prefab;
        public string Icon;
        public string Name;
        public Enum.RoleType TargetType;
        public Enum.AttrType AttrType;
    }

    //装备表
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

    //装备类型表
    [Serializable]
    public class EquipmentTypeConfig : Config
    {
        public string Name;
        public int[] Combination;
        public Enum.EquipPlace Place;
        public int Animator;
    }

    //地块表
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

    //属性表
    [Serializable]
    public class AttrConfig:Config
    {
        public string Name;
    }

    //Buff表
    [Serializable]
    public class BufferConfig:Config
    {
        public Pair Attr;
        public int Duration;
        public string Name;
        public string Desc;
    }

    //角色职业表
    [Serializable]
    public class RoleJobConfig : Config
    {
        public int[] Equipments;
        public string Name;
        public string Icon;
    }

    //动画状态机表
    [Serializable]
    public class AnimatorConfig : Config 
    {
        public string Controller;
    }

    //装备部位表
    [Serializable]
    public class EquipPlaceConfig : Config
    {
        public string SpinePoint;
    }

    //对话组表
    [Serializable]
    public class DialogGroupConfig : Config
    {
        public int MaxIndex;
    }

    //对话表
    [Serializable]
    public class DialogConfig : Config
    {
        public string Context;
        public int Role;
    }

    //关卡表
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
