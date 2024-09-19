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

    //角色表
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

    //地块表
    [Serializable]
    public class HexagonConfig : Config
    {
        public string Prefab;
        //public bool Reachable;
        public float Resistance;
    }

    //角色星级表
    [Serializable]
    public class RoleStarConfig : Config
    {
        public List<IntFloatPair> Attrs;
        public int Cost;
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
        public string Desc;
        public Enum.TargetType TargetType;
        public int[] Params;
    }

    //装备表
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

    //装备类型表
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

    //属性表
    [Serializable]
    public class AttrConfig : Config
    {
        public string Name;
        public Enum.ValueType ValueType;
    }

    //Buff表
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

    //角色职业表
    [Serializable]
    public class RoleJobConfig : Config
    {
        public Enum.EquipType[] AdeptEquipTypes;
        public string Name;
        public string Icon;
    }

    //动画状态机表
    [Serializable]
    public class AnimatorConfig : Config
    {
        public string Controller;
        public string Jump;
        public int Priority;
    }

    //装备部位表
    [Serializable]
    public class EquipPlaceConfig : Config
    {
        public string SpinePoint;
        public string ViceSpinePoint;
    }

    //对话组表
    [Serializable]
    public class DialogGroupConfig : Config
    {
        public int MaxIndex;
        public List<int> Options;
    }

    //对话表
    [Serializable]
    public class DialogConfig : Config
    {
        public string Context;
        public int Role;
    }

    //对话选项表
    [Serializable]
    public class DialogOptionConfig : Config
    {
        public string Title;
        public int Event;
    }

    //关卡表  
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

    //敌人表
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

    //敌人表
    [Serializable]
    public class HeroConfig : Config
    {
        public int RoleID;
        public int Level;
    }

    //天赋表
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

    //篝火表
    [Serializable]
    public class BonfireConfig : Config
    {
        public string Prefab;
        public List<IntFloatPair> Effects;
    }

    //元素表
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

    //故事组表
    [Serializable]
    public class StoryGroupConfig : Config
    {
        public int Count;
    }

    //故事表
    [Serializable]
    public class StoryConfig : Config
    {
        public string Context;
        public string Pic;
    }

    //道具
    [Serializable]
    public class ItemPair
    {
        //public Enum.SourceType Type;
        public int id;
        public int value;
    }

    //事件表
    [Serializable]
    public class EventConfig : Config
    {
        public Enum.EventType Type;
        public string Name;
        public int Value;
        public List<int> NextEvents;
    }

    //翻译表基类
    [Serializable]
    public class TranslationConfig
    {
        public string ID;
        public string Str;
    }

    //道具表
    [Serializable]
    public class ItemConfig : Config
    {
        public string Icon;
        public string Name;
        public string Prefab;
    }

    //奖励表
    [Serializable]
    public class RewardConfig : Config
    {
        public ItemPair[] Rewards;
    }

    //装饰物表
    [Serializable]
    public class OrnamentConfig : Config
    {
        public string Prefab;
    }

    //语言表
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
