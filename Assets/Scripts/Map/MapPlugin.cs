using System;
using UnityEngine;

namespace WarGame
{
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
        public int ID;

        public string hexagonID;

        public EnemyMapPlugin(int configId, string hexagonID)
        {
            this.ID = configId;
            this.hexagonID = hexagonID;
        }
    }

    [Serializable]
    public class BonfireMapPlugin
    {
        public string ID;

        public int configId;

        public string hexagonID;

        public BonfireMapPlugin(string ID, int configId, string hexagonID)
        {
            this.configId = configId;
            this.ID = ID;
            this.hexagonID = hexagonID;
        }
    }

    [Serializable]
    public class OrnamentMapPlugin
    {
        public string ID;
        public int configID;
        public string hexagonID;

        public OrnamentMapPlugin(string ID, int configID, string hexagonID)
        {
            this.ID = ID;
            this.configID = configID;
            this.hexagonID = hexagonID;
        }
    }

    [Serializable]
    public class LevelMapPlugin
    {
        public HexagonMapPlugin[] hexagons;
        public EnemyMapPlugin[] enemys;
        public BonfireMapPlugin[] bonfires;
        public OrnamentMapPlugin[] ornaments;

        public LevelMapPlugin(HexagonMapPlugin[] hexagons, EnemyMapPlugin[] enemys, BonfireMapPlugin[] bonfires, OrnamentMapPlugin[] ornaments)
        {
            this.hexagons = hexagons;
            this.enemys = enemys;
            this.bonfires = bonfires;
            this.ornaments = ornaments;
        }
    }
}
