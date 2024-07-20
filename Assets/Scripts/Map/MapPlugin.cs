using System;
using UnityEngine;

namespace WarGame
{
    [Serializable]
    public class HexagonMapPlugin
    {
        public int ID;

        public int configId;

        public bool isReachable;

        public WGVector3 coor;

        public HexagonMapPlugin(int id, int configId, bool isReachable, WGVector3 coor)
        {
            this.ID = id;
            this.configId = configId;
            this.isReachable = isReachable;
            this.coor = coor;
        }
    }

    [Serializable]
    public class NewEnemyMapPlugin
    {
        public int ID;

        public int hexagonID;

        public NewEnemyMapPlugin(int configId, int hexagonID)
        {
            this.ID = configId;
            this.hexagonID = hexagonID;
        }
    }

    [Serializable]
    public class BonfireMapPlugin
    {
        public int ID;

        public int configId;

        public int hexagonID;

        public BonfireMapPlugin(int ID, int configId, int hexagonID)
        {
            this.configId = configId;
            this.ID = ID;
            this.hexagonID = hexagonID;
        }
    }

    [Serializable]
    public class OrnamentMapPlugin
    {
        public int ID;
        public int configID;
        public int hexagonID;
        public float scale;
        public WGVector3 rotation;

        public OrnamentMapPlugin(int ID, int configID, int hexagonID, float scale, WGVector3 rotation)
        {
            this.ID = ID;
            this.configID = configID;
            this.hexagonID = hexagonID;
            this.scale = scale;
            this.rotation = rotation;// new Quaternion(rotation.x, rotation.y, rotation.z, rotation.z);
        }
    }

    [Serializable]
    public class LightingPlugin
    {
        public string Sky;
        public Vector4 LightColor;

        public LightingPlugin(string sky, Vector4 lightColor)
        {
            this.Sky = sky;
            this.LightColor = lightColor;
        }
    }

    [Serializable]
    public class LevelMapPlugin
    {
        public HexagonMapPlugin[] hexagons;
        public NewEnemyMapPlugin[] enemys;
        public BonfireMapPlugin[] bonfires;
        public OrnamentMapPlugin[] ornaments;
        public LightingPlugin lightingPlugin;

        public LevelMapPlugin(HexagonMapPlugin[] hexagons, NewEnemyMapPlugin[] enemys, BonfireMapPlugin[] bonfires, OrnamentMapPlugin[] ornaments, LightingPlugin lightingPlugin)
        {
            this.hexagons = hexagons;
            this.enemys = enemys;
            this.bonfires = bonfires;
            this.ornaments = ornaments;
            this.lightingPlugin = lightingPlugin;
        }
    }
}
