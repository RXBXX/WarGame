using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace WarGame
{
    public class MapTool : Singeton<MapTool>
    {
        //地图编辑场景路径
        private const string MapPath = "Assets/Scenes/MapEditorScene.unity";

        //六边形内径
        private float _insideDiameter = 1.0f;

        //z轴偏移弧度
        private const float _radian = 30.0F / 180.0F * Mathf.PI;

        //高度
        private const float _height = 0.23F;


        //public float _insideDiameter
        //{
        //    get { return _insideDiameter; }
        //}

        //public float _radian
        //{
        //    get { return _radian; }
        //}

        /// <summary>
        /// 从世界坐标转换成地图格子坐标
        /// </summary>
        public Vector3 GetCoorFromPos(Vector3 pos)
        {
            var hexMapX = 0.0f;
            var hexMapZ = 0.0f;
            if (pos.x - pos.z * Mathf.Tan(_radian) < 0)
                hexMapX = (int)((pos.x - pos.z * Mathf.Tan(_radian) - _insideDiameter / 2.0f) / _insideDiameter);
            else
                hexMapX = (int)((pos.x - pos.z * Mathf.Tan(_radian) + _insideDiameter / 2.0f) / _insideDiameter);

            if (pos.z / Mathf.Cos(_radian) < 0)
                hexMapZ = (int)((pos.z / Mathf.Cos(_radian) - _insideDiameter / 2.0f) / _insideDiameter);
            else
                hexMapZ = (int)((pos.z / Mathf.Cos(_radian) + _insideDiameter / 2.0f) / _insideDiameter);

            var hexMapY = (int)((pos.y + 0.01F) / _height);

            return new Vector3(hexMapX, hexMapY, hexMapZ);
        }

        /// <summary>
        /// 从地图格子左边转换成世界坐标
        /// </summary>
        /// <param name="coor"></param>
        /// <returns></returns>
        public Vector3 GetPosFromCoor(Vector3 coor)
        {
            var hexPosZ = coor.z * _insideDiameter * Mathf.Cos(_radian);
            var hexPosX = coor.x * _insideDiameter + hexPosZ * Mathf.Tan(_radian);
            var hexPosY = coor.y * _height;

            return new Vector3(hexPosX, hexPosY, hexPosZ);
        }


        /// <summary>
        /// 创建地图
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="root"></param>
        public Dictionary<string, HexagonCell> CreateMap(string dir, GameObject root)
        {
            var jsonStr = File.ReadAllText(dir);
            HexagonCell[] hexagons = Tool.Instance.FromJson<HexagonCell[]>(jsonStr);
            Dictionary<string, HexagonCell> hexagonDic = new Dictionary<string, HexagonCell>();
            for (int i = 0; i < hexagons.Length; i++)
            {
                var hexagon = hexagons[i];
                hexagon.CreateGameObject();
                hexagon.SetParent(root.transform);

                hexagonDic[GetHexagonKey(hexagon.coordinate)] = hexagon;
            }
            return hexagonDic;
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

        /// <summary>
        /// 打开地图编辑专用的场景
        /// </summary>
        public void OpenEditorMapScene()
        {
            if (Application.isPlaying)
                return;

            EditorSceneManager.OpenScene(MapPath);
        }

        /// <summary>
        /// 导出地图的接口
        /// </summary>
        public void SaveEditorMap()
        {
            if (!IsActiveMapEditor())
                return;

            var dir = EditorUtility.SaveFilePanel("导出地图", Application.dataPath + "/Maps", "地图", "json");

            var rootObj = GameObject.Find("Root");
            var hexagonCount = rootObj.transform.childCount;
            HexagonCell[] hexagons = new HexagonCell[hexagonCount];

            for (int i = 0; i < hexagonCount; i++)
            {
                var hexagonTra = rootObj.transform.GetChild(i);
                var data = hexagonTra.GetComponent<HexagonCellData>();
                var coor = GetCoorFromPos(hexagonTra.position);
                var hexagonCell = new HexagonCell(GetHexagonKey(coor), data.config);
                hexagonCell.coordinate = coor;
                hexagons[i] = hexagonCell;
            }

            var jsonStr = WarGame.Tool.Instance.ToJson(hexagons);
            try { File.WriteAllText(dir, jsonStr); }
            catch (IOException exception)
            {
                Debug.Log(exception);
            };
        }

        /// <summary>
        /// 打开地图的接口
        /// </summary>
        public void OpenEditorMap()
        {
            if (!IsActiveMapEditor())
                return;

            ClearEditorMapScene();

            var dir = EditorUtility.OpenFilePanel("打开地图", Application.dataPath + "/Maps", "");
            var jsonStr = File.ReadAllText(dir);

            var rootObj = GameObject.Find("Root");
            HexagonCell[] hexagons = WarGame.Tool.Instance.FromJson<HexagonCell[]>(jsonStr);

            for (int i = 0; i < hexagons.Length; i++)
            {
                hexagons[i].CreateGameObject();
                hexagons[i].SetParent(rootObj.transform);
            }
        }

        /// <summary>
        /// 快速生成地图
        /// </summary>
        public void QuickGenerageEditorMap(int xNum, int yNum, int zNum, Dictionary<Enum.HexagonType, int> weightList)
        {
            if (!IsActiveMapEditor())
                return;

            ClearEditorMapScene();

            var rootObj = GameObject.Find("Root");

            Dictionary<int, Enum.HexagonType> castDic = new Dictionary<int, Enum.HexagonType>();
            int weightBase = 0;
            foreach (var pair in weightList)
            {
                weightBase += pair.Value;
                castDic[weightBase] = pair.Key;
            }

            for (int i = 0; i < xNum; i++)
            {
                for (int j = 0; j < zNum; j++)
                {
                    for (int q = 0; q < yNum; q++)
                    {
                        var rd = Random.Range(0, weightBase);
                        var minKey = weightBase;
                        Enum.HexagonType type = Enum.HexagonType.BeachShore;
                        foreach (var pair in castDic)
                        {
                            if (rd < pair.Key && pair.Key < minKey)
                            {
                                minKey = pair.Key;
                                type = pair.Value;
                            }
                        }

                        string assetPath = MapTool.Instance.GetHexagonPrefab(type);
                        GameObject prefab = AssetMgr.Instance.LoadAsset<GameObject>(assetPath);
                        var obj = GameObject.Instantiate(prefab);
                        obj.transform.position = MapTool.Instance.GetPosFromCoor(new Vector3(i, q, j));
                        obj.transform.SetParent(rootObj.transform);
                    }
                }
            }
        }

        /// <summary>
        /// 清空场景
        /// </summary>
        public void ClearEditorMapScene()
        {
            if (!IsActiveMapEditor())
                return;

            var rootObj = GameObject.Find("Root");
            var hexagonCount = rootObj.transform.childCount;

            for (int i = hexagonCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(rootObj.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 获取地块在构建的地图数据中的key
        /// </summary>
        /// <returns></returns>
        public string GetHexagonKey(Vector3 pos)
        {
            return pos.x + "_" + pos.y + "_" + pos.z;
        }
    }
}
