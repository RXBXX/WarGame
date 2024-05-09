using System.Collections.Generic;
using System;

namespace WarGame
{
    [Serializable]
    public class EquipmentData
    {
        public int UID;
        public int configId;

        public EquipmentData(int UID, int configId)
        {
            this.UID = UID;
            this.configId = configId;
        }

        public EquipmentData Clone()
        {
            return new EquipmentData(this.UID, this.configId);
        }
    }

    [Serializable]
    public class RoleData
    {
        public int UID;
        public int configId;
        public int level;
        public Dictionary<Enum.EquipPlace, int> equipmentDic = new Dictionary<Enum.EquipPlace, int>();
        public Dictionary<int, int> skillDic = new Dictionary<int, int>();

        public RoleData(int UID, int configId, int level, Dictionary<Enum.EquipPlace, int> equipmentDic, Dictionary<int, int> skillDic)
        {
            this.UID = UID;
            this.configId = configId;
            this.level = level;
            this.equipmentDic = equipmentDic;
            this.skillDic = skillDic;
        }

        public RoleData Clone()
        {
            var cloneEquipDic = new Dictionary<Enum.EquipPlace, int>();
            foreach (var v in this.equipmentDic)
            {
                cloneEquipDic.Add(v.Key, v.Value);
            }
            var cloneSkillDic = new Dictionary<int, int>();
            foreach (var v in this.skillDic)
            {
                cloneSkillDic.Add(v.Key, v.Value);
            }
            return new RoleData(this.UID, this.configId, this.level, cloneEquipDic, cloneSkillDic);
        }
    }

    /// <summary>
    /// 关卡中角色数据
    /// </summary>
    [Serializable]
    public class LevelRoleData
    {
        public float UID;
        public int configId;
        public int level;
        public float hp;
        public string hexagonID;
        public Dictionary<Enum.EquipPlace, int> equipmentDic = new Dictionary<Enum.EquipPlace, int>();
        public Dictionary<int, int> skillDic = new Dictionary<int, int>();
        public List<Pair> _buffs = new List<Pair>();
    }

    /// <summary>
    /// 关卡数据
    /// </summary>
    [Serializable]
    public class LevelData 
    {
        public int configId;
        public bool pass;
        public List<LevelRoleData> heros = new List<LevelRoleData>();
        public List<LevelRoleData> enemys = new List<LevelRoleData>();
    }

    [Serializable]
    public class GameData
    {
        public string title;
        public float time;
        public Dictionary<int, RoleData> roleDataDic = new Dictionary<int, RoleData>();
        public Dictionary<int, EquipmentData> equipDataDic = new Dictionary<int, EquipmentData>();
        public Dictionary<int, LevelData> levelDataDic = new Dictionary<int, LevelData>();

        public GameData(string title, float time)
        {
            this.title = title;
            this.time = time;
            var skillDic = new Dictionary<int, int>();
            skillDic.Add(10001, 1);
            skillDic.Add(10002, 1);
            roleDataDic.Add(1, new RoleData(1, 10001, 1, new Dictionary<Enum.EquipPlace, int>(), skillDic));
            roleDataDic.Add(2, new RoleData(2, 10002, 1, new Dictionary<Enum.EquipPlace, int>(), skillDic));
             
            equipDataDic.Add(1, new EquipmentData(1, 10001));
            equipDataDic.Add(2, new EquipmentData(2, 10002));
            equipDataDic.Add(3, new EquipmentData(3, 10003));
            equipDataDic.Add(4, new EquipmentData(4, 10004));
            equipDataDic.Add(5, new EquipmentData(5, 10005));
            equipDataDic.Add(6, new EquipmentData(6, 10006));
            equipDataDic.Add(7, new EquipmentData(7, 10007));
            equipDataDic.Add(8, new EquipmentData(8, 10008));
            equipDataDic.Add(9, new EquipmentData(9, 10009));
            equipDataDic.Add(10, new EquipmentData(10, 10010));
            equipDataDic.Add(11, new EquipmentData(11, 10011));
            equipDataDic.Add(12, new EquipmentData(12, 10012));
            equipDataDic.Add(13, new EquipmentData(13, 10013));
        }

        public GameData Clone()
        {
            var gd = new GameData(this.title, this.time);
            foreach (var v in roleDataDic)
            {
                gd.roleDataDic.Add(v.Key, v.Value.Clone());
            }
            foreach (var v in equipDataDic)
            {
                gd.equipDataDic.Add(v.Key, v.Value.Clone());
            }
            return gd;
        }
    }

    public class SampleGameData
    {
        public string title;
        public float time;

        public SampleGameData(string title, float time)
        {
            this.title = title;
            this.time = time;
        }
    }
}
