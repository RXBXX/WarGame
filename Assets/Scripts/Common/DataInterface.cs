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

    [Serializable]
    public class GameData
    {
        public string title;
        public float time;
        public Dictionary<int, RoleData> roleData = new Dictionary<int, RoleData>();
        public Dictionary<int, EquipmentData> equipmentData = new Dictionary<int, EquipmentData>();

        public GameData(string title, float time)
        {
            this.title = title;
            this.time = time;
            var skillDic = new Dictionary<int, int>();
            skillDic.Add(10001, 1);
            skillDic.Add(10002, 1);
            roleData.Add(1, new RoleData(1, 10001, 1, new Dictionary<Enum.EquipPlace, int>(), skillDic));
            roleData.Add(2, new RoleData(2, 10002, 1, new Dictionary<Enum.EquipPlace, int>(), skillDic));

            equipmentData.Add(1, new EquipmentData(1, 10001));
            equipmentData.Add(2, new EquipmentData(2, 10002));
            equipmentData.Add(3, new EquipmentData(3, 10003));
            equipmentData.Add(4, new EquipmentData(4, 10004));
            equipmentData.Add(5, new EquipmentData(5, 10005));
            equipmentData.Add(6, new EquipmentData(6, 10006));
            equipmentData.Add(7, new EquipmentData(7, 10007));
            equipmentData.Add(8, new EquipmentData(8, 10008));
            equipmentData.Add(9, new EquipmentData(9, 10009));
            equipmentData.Add(10, new EquipmentData(10, 10010));
            equipmentData.Add(11, new EquipmentData(11, 10011));
            equipmentData.Add(12, new EquipmentData(12, 10012));
            equipmentData.Add(13, new EquipmentData(13, 10013));
        }

        public GameData Clone()
        {
            var gd = new GameData(this.title, this.time);
            foreach (var v in roleData)
            {
                gd.roleData.Add(v.Key, v.Value.Clone());
            }
            foreach (var v in equipmentData)
            {
                gd.equipmentData.Add(v.Key, v.Value.Clone());
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
