using System;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    [Serializable]
    public class Config
    {
        public int ID;

        public string GetTranslation(string key)
        {
            var configName = GetType().ToString().Split('.')[1];
            return ConfigMgr.Instance.GetTranslation(configName, ID, key);
        }
    }

    //��ɫ��
    [Serializable]
    public class RoleConfig : Config
    {
        public int Job;
        public string Name;
        public string Desc;
        public string Prefab;
        public int CommonSkill;
        public int SpecialSkill;
        public string Icon;
        public string FullLengthIcon;
        public Enum.Element Element;
        public int MaxLevel;
        public string DeadSound;
        public string IntoBattleSound;
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
        public int Cost;
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
        public int[] Params;
    }

    //װ����
    [Serializable]
    public class EquipmentConfig : Config
    {
        public Enum.EquipType Type;
        public string Icon;
        public string FullLengthIcon;
        public string Prefab;
        public string VicePrefab;
        public string Name;
        public List<IntFloatPair> Attrs;
        public List<IntFloatPair> Buffs;
        public string Effect;
        public string Bullet;
        public string HitEffect;
        public int Cost;
        public string Sound;
    }

    //װ�����ͱ�
    [Serializable]
    public class EquipmentTypeConfig : Config
    {
        public string Name;
        public Enum.EquipType[] Combination;
        public Enum.EquipPlace Place;
        public int Animator;
        public Vector3 Pos;
        public Vector3 Rotation;
        public Vector3 ViceRotation;
        public string Icon;
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
        public Enum.BuffAttrEffectType EffectType;
        public int Duration;
        public string Name;
        public string Desc;
        public string Icon;
        public string Effect;
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
        public int[] WinCond;
        public string Music;
        public Enum.Element Element;
        public string TargetDesc;
        public string Icon;
        public int HeroCount;
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
        public string Group;
        public bool IsBoss;
        public int Reward;
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
        public string Name;
        public string Desc;
        public List<IntFloatPair> Attrs;
        public int LastTalent;
        public int Cost;
        public int Level;
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
        public float RestrainValue;
        public Enum.Element Reinforce;
        public float ReinforceValue;
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

    //����
    [Serializable]
    public class ItemPair
    {
        //public Enum.SourceType Type;
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
        public List<int> NextEvents;
    }

    //��������
    [Serializable]
    public class TranslationConfig
    {
        public string ID;
        public string Str;
    }

    //���߱�
    [Serializable]
    public class ItemConfig : Config
    {
        public string Icon;
        public string Name;
        public string Prefab;
    }

    //������
    [Serializable]
    public class RewardConfig : Config
    {
        public ItemPair[] Rewards;
    }

    //װ�����
    [Serializable]
    public class OrnamentConfig : Config
    {
        public string Prefab;
    }

    //���Ա�
    [Serializable]
    public class LanguageConfig : Config
    {
        public string Name;
        public string SimpleName;
    }

    [Serializable]
    public class PlayGameConfig : Config
    {
        public string Desc;
    }
}
