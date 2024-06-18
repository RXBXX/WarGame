using UnityEngine;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

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
        private const float _height = 0.2F;


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

            var hexMapY = (int)((pos.y ) / _height);

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
        public Dictionary<string, Hexagon> CreateMap(HexagonMapPlugin[] hexagons, GameObject root)
        {
            Dictionary<string, Hexagon> hexagonDic = new Dictionary<string, Hexagon>();
            for (int i = 0; i < hexagons.Length; i++)
            {
                var hexagon = new Hexagon(hexagons[i].ID, hexagons[i].configId, hexagons[i].isReachable, hexagons[i].coor);
                hexagon.SetParent(root.transform);
                hexagonDic[GetHexagonKey(hexagon.coor)] = hexagon;
            }
            return hexagonDic;
        }

        public Dictionary<int, Bonfire> CreateBonfire(BonfireMapPlugin[] bonfires, GameObject root)
        {
            Dictionary<int, Bonfire> bonfireDic = new Dictionary<int, Bonfire>();
            if (null != bonfireDic)
            {
                for (int i = 0; i < bonfires.Length; i++)
                {
                    var bonfire = new Bonfire(i, bonfires[i].configId, bonfires[i].hexagonID);
                    bonfire.SetParent(root.transform);
                    bonfireDic[i] = bonfire;
                }
            }
            return bonfireDic;
        }

#if UNITY_EDITOR

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


            var rootObj = GameObject.Find("Root");
            var hexagonCount = rootObj.transform.childCount;
            HexagonMapPlugin[] hexagons = new HexagonMapPlugin[hexagonCount];
            for (int i = 0; i < hexagonCount; i++)
            {
                var hexagonTra = rootObj.transform.GetChild(i);
                var data = hexagonTra.GetComponent<HexagonBehaviour>();
                var coor = GetCoorFromPos(hexagonTra.position);
                var hexagonCell = new HexagonMapPlugin(GetHexagonKey(coor), data.configId, data.IsReachable, coor);
                hexagons[i] = hexagonCell;
            }

            var roleRootObj = GameObject.Find("RoleRoot");
            var enemyCount = roleRootObj.transform.childCount;
            EnemyMapPlugin[] enemys = new EnemyMapPlugin[enemyCount];
            for (int i = 0; i < enemyCount; i++)
            {
                var enemyTra = roleRootObj.transform.GetChild(i);
                var data = enemyTra.GetComponent<RoleBehaviour>();
                enemys[i] = new EnemyMapPlugin(data.ID, GetHexagonKey(GetCoorFromPos(enemyTra.position - CommonParams.Offset)));
            }

            var fireRootObj = GameObject.Find("BonfireRoot");
            var bonfireCount = fireRootObj.transform.childCount;
            BonfireMapPlugin[] bonfires = new BonfireMapPlugin[bonfireCount];
            for (int i = 0; i < bonfireCount; i++)
            {
                var bonfireTra = fireRootObj.transform.GetChild(i);
                var data = bonfireTra.GetComponent<BonfireBehaviour>();
                bonfires[i] = new BonfireMapPlugin(data.ID, GetHexagonKey(GetCoorFromPos(bonfireTra.position - CommonParams.Offset)));
            }

            var levelPlugin = new LevelMapPlugin(hexagons, enemys, bonfires);

            var dir = EditorUtility.SaveFilePanel("导出地图", Application.dataPath + "/Maps", "地图", "json");
            Tool.Instance.WriteJson<LevelMapPlugin>(dir, levelPlugin);
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
            LevelMapPlugin levelPlugin = Tool.Instance.ReadJson<LevelMapPlugin>(dir);

            MapManager.Instance.CreateMap(levelPlugin.hexagons, levelPlugin.bonfires);

            RoleManager.Instance.InitLevelRoles(levelPlugin.enemys);
        }

        /// <summary>
        /// 快速生成地图
        /// </summary>
        public void QuickGenerageEditorMap(int xNum, int yNum, int zNum, Dictionary<Enum.HexagonType, int> weightList)
        {
            //DebugManager.Instance.Log("QuickGenerageEditorMap");
            if (!IsActiveMapEditor())
                return;

            //DebugManager.Instance.Log("QuickGenerageEditorMap111");
            ClearEditorMapScene();

            var rootObj = GameObject.Find("Root");

            Dictionary<int, Enum.HexagonType> castDic = new Dictionary<int, Enum.HexagonType>();
            int weightBase = 0;
            foreach (var pair in weightList)
            {
                if (0 != pair.Value)
                {
                    weightBase += pair.Value;
                    castDic[weightBase] = pair.Key;
                }
            }

            for (int i = 0; i < xNum; i++)
            {
                for (int j = 0; j < zNum; j++)
                {
                    for (int q = 0; q < yNum; q++)
                    {
                        var rd = Random.Range(0, weightBase);
                        var minKey = weightBase;
                        Enum.HexagonType type = Enum.HexagonType.Hex1;
                        foreach (var pair in castDic)
                        {
                            if (rd < pair.Key && pair.Key <= minKey)
                            {
                                minKey = pair.Key;
                                type = pair.Value;
                            }
                        }

                        string assetPath = ConfigMgr.Instance.GetConfig<HexagonConfig>("HexagonConfig", (int)type).Prefab;
                        AssetsMgr.Instance.LoadAssetAsync<GameObject>(assetPath, (GameObject prefab) =>
                        {
                            var obj = GameObject.Instantiate(prefab);
                            obj.transform.position = MapTool.Instance.GetPosFromCoor(new Vector3(i, q, j));
                            obj.transform.SetParent(rootObj.transform);
                        });
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

            var roleRootObj = GameObject.Find("RoleRoot");
            var enemyCount = roleRootObj.transform.childCount;
            for (int i = enemyCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(roleRootObj.transform.GetChild(i).gameObject);
            }

            var fireRootObj = GameObject.Find("BonfireRoot");
            var bonfireCount = fireRootObj.transform.childCount;
            for (int i = bonfireCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(fireRootObj.transform.GetChild(i).gameObject);
            }
        }
#endif
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
