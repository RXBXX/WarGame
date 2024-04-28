using System.Collections.Generic;
using System;

namespace WarGame
{
    [Serializable]
    public struct EquipmentData
    {
        public int UID;
        public int configId;

        public EquipmentData(int UID, int configId)
        {
            this.UID = UID;
            this.configId = configId;
        }
    }

    [Serializable]
    public struct RoleData
    {
        public int UID;
        public int configId;
        public int level;
        public Dictionary<Enum.EquipmentType, EquipmentData> equipmentDic;
        public Dictionary<int, int> skillDic;

        public RoleData(int UID, int configId, int level, Dictionary<Enum.EquipmentType, EquipmentData> equipmentDic, Dictionary<int, int> skillDic)
        {
            this.UID = UID;
            this.configId = configId;
            this.level = level;
            this.equipmentDic = equipmentDic;
            this.skillDic = skillDic;
        }
    }

    [Serializable]
    public struct GameData
    {
        public string title;
        public float time;
        public Dictionary<int, RoleData> roleData;
        public Dictionary<int, EquipmentData> equipmentData;

        public GameData(string title, float time)
        {
            this.title = title;
            this.time = time;

            roleData = new Dictionary<int, RoleData>();
            var skillDic = new Dictionary<int, int>();
            skillDic.Add(10001, 1);
            skillDic.Add(10002, 1);
            roleData.Add(1, new RoleData(1, 10001, 1, null, skillDic));
            roleData.Add(2, new RoleData(2, 10002, 1, null, skillDic));

            equipmentData = new Dictionary<int, EquipmentData>();
            equipmentData.Add(1, new EquipmentData(1, 10001));
            equipmentData.Add(2, new EquipmentData(2, 10002));
            equipmentData.Add(3, new EquipmentData(3, 10003));
            equipmentData.Add(4, new EquipmentData(4, 10004));
        }
    }

    public struct SampleGameData
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
