using System;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    [Serializable]
    public struct IntFloatPair
    {
        public int id;
        public float value;

        public IntFloatPair(int id, float value)
        {
            this.id = id;
            this.value = value;
        }
    }

    [Serializable]
    public struct StringStringPair
    {
        public string id;
        public string value;

        public StringStringPair(string id, string value)
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
        public Enum.Element Element;
    }

    //�ؿ��
    [Serializable]
    public class HexagonConfig : Config
    {
        public string Prefab;
        //public bool Reachable;
        public float Resistance;
    }

    //��ɫ�Ǽ���
    [Serializable]
    public class RoleStarConfig : Config
    {
        public List<IntFloatPair> Attrs;
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
        public string VicePrefab;
        public string Name;
        public List<IntFloatPair> Attrs;
        public List<IntFloatPair> Buffs;
        public string Effect;
        public string Bullet;
        public string HitEffect;
    }

    //װ�����ͱ�
    [Serializable]
    public class EquipmentTypeConfig : Config
    {
        public string Name;
        public Enum.EquipType[] Combination;
        public Enum.EquipPlace Place;
        public int Animator;
        public Vector3 Rotation;
        public Vector3 ViceRotation;
    }

    [Serializable]
    public class HexagonMapPlugin
    {
        public string ID;

        public int configId;

        public bool isReachable;

        public Vector3 coor;

        public HexagonMapPlugin(string id, int configId, bool isReachable, Vector3 coor)
        {
            this.ID = id;
            this.configId = configId;
            this.isReachable = isReachable;
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
        public Enum.ValueType ValueType;
    }

    //Buff��
    [Serializable]
    public class BufferConfig : Config
    {
        public IntFloatPair Attr;
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
        public string ViceSpinePoint;
    }

    //�Ի����
    [Serializable]
    public class DialogGroupConfig : Config
    {
        public int MaxIndex;
        public List<int> Options;
        public int Event;
    }

    //�Ի���
    [Serializable]
    public class DialogConfig : Config
    {
        public string Context;
        public int Role;
    }

    //�Ի�ѡ���
    [Serializable]
    public class DialogOptionConfig : Config
    {
        public string Title;
        public int Event;
    }

    //�ؿ���
    [Serializable]
    public class LevelConfig : Config
    {
        public string Name;
        public Vector2 UIPos;
        public string Desc;
        public Enum.LevelType Type;
        public string Map;
        public int StartEvent;
        public int WinEvent;
        public int FailedEvent;
        //public SourcePair[] Rewards;
    }

    //���˱�
    [Serializable]
    public class EnemyConfig : Config
    {
        public int RoleID;
        public int Level;
        public int[] Equips;
        public int NextStage;
        public int DefeatEvent;
        public float ViewDis;
    }

    //���˱�
    [Serializable]
    public class HeroConfig : Config
    {
        public int RoleID;
        public int Level;
    }

    //�츳��
    [Serializable]
    public class TalentConfig : Config
    {
        public int Group;
        public string Name;
        public string Desc;
        public List<IntFloatPair> Attrs;
        public int LastTalent;
        public int Place;
    }

    //�����
    [Serializable]
    public class BonfireConfig : Config
    {
        public string Prefab;
        public List<IntFloatPair> Effects;
    }

    //Ԫ�ر�
    [Serializable]
    public class ElementConfig : Config
    {
        public string Name;
        public Enum.Element Restrain;
        public Enum.Element Reinforce;
        public string Icon;
    }

    //�������
    [Serializable]
    public class StoryGroupConfig : Config
    {
        public int Count;
    }

    //���±�
    [Serializable]
    public class StoryConfig : Config
    {
        public string Context;
        public string Pic;
    }

    //��Դ����
    [Serializable]
    public class SourcePair
    {
        public Enum.SourceType Type;
        public int id;
        public int value;
    }

    //�¼���
    [Serializable]
    public class EventConfig : Config
    {
        public Enum.EventType Type;
        public string Name;
        public int Value;
    }
}
