using System;
using UnityEngine;

namespace WarGame
{
    [Serializable]
    public class Config
    {
        public int ID;
    }

    [Serializable]
    public class RoleConfig: Config
    {
        public string Name;
        public string Prefab;
        public int StarGroup;
    }

    [Serializable]
    public class HexagonConfig : Config
    {
        public string Prefab;
        public bool Reachable;
        public float Resistance;
    }

    [Serializable]
    public class RoleStarConfig : Config
    {
        public float MoveDis;
        public float AttackDis;
        public float HP;
        public float Attack;
        public float Defense;
    }

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
}
