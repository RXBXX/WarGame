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

    //��ɫ��
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
        public float MoveDis;
        public float AttackDis;
        public float HP;
        public float Attack;
        public float Defense;
        public List<AttrStruct> Attrs;
    }

    //���ܵȼ���
    [Serializable]
    public class SkillLevelConfig : Config
    {
        public AttrStruct Attr;
    }

    //���ܱ�
    [Serializable]
    public class SkillConfig : Config
    {
        public string Prefab;
        public string Icon;
        public string Name;
        public Enum.RoleType TargetType;
    }

    //װ����
    [Serializable]
    public class EquipmentConfig : Config
    {
        public int Type;
        public string Icon;
        public string Prefab;
        public string Name;
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
        public AttrStruct Attr;
        public int Duration;
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
}
