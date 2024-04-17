using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace WarGame
{
    public class MapTool : Singeton<MapTool>
    {
        //六边形内径
        private float _insideDiameter = 1.0f;

        //z轴偏移弧度
        private const float _radian = 30.0F / 180.0F * Mathf.PI;

        //高度
        private const float _height = 0.23F;


        public float InsideDiameter
        {
            get { return _insideDiameter; }
        }

        public float Radian
        {
            get { return _radian; }
        }

        /// <summary>
        /// 从世界坐标转换成地图格子坐标
        /// </summary>
        public Vector3 FromWorldPosToCellPos(Vector3 pos)
        {
            var hexMapX = 0.0f;
            var hexMapZ = 0.0f;
            if (pos.x - pos.z * Mathf.Tan(Radian) < 0)
                hexMapX = (int)((pos.x - pos.z * Mathf.Tan(Radian) - InsideDiameter / 2.0f) / InsideDiameter);
            else
                hexMapX = (int)((pos.x - pos.z * Mathf.Tan(Radian) + InsideDiameter / 2.0f) / InsideDiameter);

            if (pos.z / Mathf.Cos(Radian) < 0)
                hexMapZ = (int)((pos.z / Mathf.Cos(Radian) - InsideDiameter / 2.0f) / InsideDiameter);
            else
                hexMapZ = (int)((pos.z / Mathf.Cos(Radian) + InsideDiameter / 2.0f) / InsideDiameter);

            var hexMapY = (int)(pos.y / _height);

            return new Vector3(hexMapX, hexMapY, hexMapZ);
        }

        /// <summary>
        /// 从地图格子左边转换成世界坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector3 FromCellPosToWorldPos(Vector3 pos)
        {
            var hexPosZ = pos.z * InsideDiameter * Mathf.Cos(Radian);
            var hexPosX = pos.x * InsideDiameter + hexPosZ * Mathf.Tan(Radian);
            var hexPosY = pos.y * _height;

            return new Vector3(hexPosX, hexPosY, hexPosZ);
        }

        public void CreateMap(string dir, GameObject root)
        {
            var jsonStr = File.ReadAllText(dir);

            HexagonCell[] hexagons = WarGame.Tool.Instance.FromJson<HexagonCell[]>(jsonStr);
            Dictionary<Enum.HexagonType, GameObject> prefabDic = new Dictionary<Enum.HexagonType, GameObject>();

            for (int i = 0; i < hexagons.Length; i++)
            {
                var hexagon = hexagons[i];
                string assetPath = "";
                GameObject prefab = null;
                if (!prefabDic.TryGetValue(hexagon.config.type, out prefab))
                {
                    assetPath = MapTool.Instance.GetHexagonPrefab(hexagon.config.type);
                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    prefabDic[hexagon.config.type] = prefab;
                }

                var obj = GameObject.Instantiate(prefab);
                obj.transform.position = MapTool.Instance.FromCellPosToWorldPos(hexagon.position);
                obj.transform.SetParent(root.transform);
            }
        }


        /// <summary>
        /// 是否开启地图编辑模式
        /// </summary>
        /// <returns></returns>
        public bool IsActiveMapEditor()
        {
            if (Application.isPlaying)
                return false;
            var curScene = EditorSceneManager.GetActiveScene();
            return curScene.name == "MapEditorScene";
        }

        ///获取格子预制体
        public string GetHexagonPrefab(Enum.HexagonType type)
        {
            string assetPath = "";
            switch (type)
            {
                case Enum.HexagonType.BeachShore:
                    assetPath = "Assets/Low Poly Hexagons/Assets/Prefabs/Hexagons/BeachShore.prefab";
                    break;
                case Enum.HexagonType.DigSite:
                    assetPath = "Assets/Low Poly Hexagons/Assets/Prefabs/Hexagons/DigSite.prefab";
                    break;
                case Enum.HexagonType.HellLake:
                    assetPath = "Assets/Low Poly Hexagons/Assets/Prefabs/Hexagons/HellLake.prefab";
                    break;
                case Enum.HexagonType.Lake:
                    assetPath = "Assets/Low Poly Hexagons/Assets/Prefabs/Hexagons/Lake.prefab";
                    break;
            }
            return assetPath;
        }
    }
}
