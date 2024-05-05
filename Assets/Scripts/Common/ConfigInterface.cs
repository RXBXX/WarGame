using System;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    [Serializable]
    public struct AttrStruct
    {
        public int id;
        public float value;
    }

    [Serializable]
    public class Config
    {
        public int ID;
    }

    [Serializable]
    public struct SkillStruct
    {
        public int id;
        public int level;
    }

    //角色表
    [Serializable]
    public class RoleConfig: Config
    {
        public int Job;
        public string Name;
        public string Prefab;
        public Enum.RoleType Type;
        public SkillStruct CommonSkill;
        public SkillStruct SpecialSkill;
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
        public float MoveDis;
        public float AttackDis;
        public float HP;
        public float Attack;
        public float Defense;
        public List<AttrStruct> Attrs;
    }

    //技能等级表
    [Serializable]
    public class SkillLevelConfig : Config
    {
        public AttrStruct Attr;
    }

    //技能表
    [Serializable]
    public class SkillConfig : Config
    {
        public string Prefab;
        public string Icon;
        public string Name;
        public Enum.RoleType TargetType;
    }

    //装备表
    [Serializable]
    public class EquipmentConfig : Config
    {
        public int Type;
        public string Icon;
        public string Prefab;
        public string Name;
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
        public AttrStruct Attr;
        public int Duration;
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
}
