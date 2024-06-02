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
    public class RoleConfig : Config
    {
        public int Job;
        public string Name;
        public string Prefab;
        public Enum.RoleType Type;
        public int CommonSkill;
        public int SpecialSkill;
        public string DialogIcon;
        public int TalentGroup;
        public string Icon;
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
        public string Desc;
        public Enum.TargetType TargetType;
        public List<Enum.AttrType> Effects;
    }

    //װ����
    [Serializable]
    public class EquipmentConfig : Config
    {
        public Enum.EquipType Type;
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
        public Enum.EquipType ForcedCombination;
        public Enum.EquipType[] Combination;
        public Enum.EquipPlace Place;
        public int Animator;
    }

    [Serializable]
    public class HexagonMapPlugin
    {
        public string ID;

        public int configId;

        public Vector3 coor;

        public HexagonMapPlugin(string id, int configId, Vector3 coor)
        {
            this.ID = id;
            this.configId = configId;
            this.coor = coor;
        }
    }

    [Serializable]
    public class EnemyMapPlugin
    {
        public int configId;

        public string hexagonID;

        public EnemyMapPlugin(int configId, string hexagonID)
        {
            this.configId = configId;
            this.hexagonID = hexagonID;
        }
    }

    [Serializable]
    public class BonfireMapPlugin
    {
        public int configId;

        public string hexagonID;

        public BonfireMapPlugin(int configId, string hexagonID)
        {
            this.configId = configId;
            this.hexagonID = hexagonID;
        }
    }

    [Serializable]
    public class LevelMapPlugin
    {
        public HexagonMapPlugin[] hexagons;
        public EnemyMapPlugin[] enemys;
        public BonfireMapPlugin[] bonfires;

        public LevelMapPlugin(HexagonMapPlugin[] hexagons, EnemyMapPlugin[] enemys, BonfireMapPlugin[] bonfires)
        {
            this.hexagons = hexagons;
            this.enemys = enemys;
            this.bonfires = bonfires;
        }
    }

    //���Ա�
    [Serializable]
    public class AttrConfig : Config
    {
        public string Name;
        public int TargetAttr;
        public Enum.ValueType ValueType;
        public int OppositeAttr;
        public Enum.AttrTargetType TargetType;
    }

    //Buff��
    [Serializable]
    public class BufferConfig : Config
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
        public Enum.EquipType[] AdeptEquipTypes;
        public string Name;
        public string Icon;
    }

    //����״̬����
    [Serializable]
    public class AnimatorConfig : Config
    {
        public string Controller;
        public string Jump;
        public int Priority;
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
        public int StartDialog;
        public int WinDialog;
        public int FailedDialog;
    }

    [Serializable]
    public class LevelEnemyConfig : Config
    {
        public int RoleID;
        public int Level;
        public float HP;
        public int[] Equips;
    }

    //�츳��
    [Serializable]
    public class TalentConfig : Config
    {
        public int Group;
        public string Name;
        public string Desc;
        public List<Pair> Attrs;
        public int LastTalent;
        public int Place;
    }

    //�����
    [Serializable]
    public class BonfireConfig : Config
    {
        public string Prefab;
        public List<Pair> Effects;
    }
}
