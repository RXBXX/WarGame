using UnityEngine;
using System.Collections.Generic;
using System.IO;
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
        private float _insideDiameter = 1f;

        //z轴偏移弧度
        private const float _radian = 30.0F / 180.0F * Mathf.PI;

        //高度
        private const float _height = 0.2F;

        /// <summary>
        /// 从世界坐标转换成地图格子坐标
        /// </summary>
        public WGVector3 GetCoorFromPos(Vector3 pos)
        {
            if (pos.x < 0)
                pos.x -= 0.0001F;
            else
                pos.x += 0.0001f;

            if (pos.y < 0)
                pos.y -= 0.0001F;
            else
                pos.y += 0.0001F;

            if (pos.z < 0)
                pos.z -= 0.0001F;
            else
                pos.z -= 0.0001F;

            var hexMapX = 0;
            var hexMapZ = 0;
            if (pos.x - pos.z * Mathf.Tan(_radian) < 0)
                hexMapX = (int)((pos.x - pos.z * Mathf.Tan(_radian) - _insideDiameter / 2.0f) / _insideDiameter);
            else
                hexMapX = (int)((pos.x - pos.z * Mathf.Tan(_radian) + _insideDiameter / 2.0f) / _insideDiameter);

            if (pos.z / Mathf.Cos(_radian) < 0)
                hexMapZ = (int)((pos.z / Mathf.Cos(_radian) - _insideDiameter / 2.0f) / _insideDiameter);
            else
                hexMapZ = (int)((pos.z / Mathf.Cos(_radian) + _insideDiameter / 2.0f) / _insideDiameter);

            var hexMapY = (int)((pos.y) / CommonParams.Offset.y);

            return new WGVector3(hexMapX, hexMapY, hexMapZ);
        }

        /// <summary>
        /// 从地图格子左边转换成世界坐标
        /// </summary>
        /// <param name="coor"></param>
        /// <returns></returns>
        public Vector3 GetPosFromCoor(WGVector3 coor)
        {
            var hexPosZ = coor.z * _insideDiameter * Mathf.Cos(_radian);
            var hexPosX = coor.x * _insideDiameter + hexPosZ * Mathf.Tan(_radian);
            var hexPosY = coor.y * CommonParams.Offset.y;

            return new Vector3(hexPosX, hexPosY, hexPosZ);
        }


        /// <summary>
        /// 创建地图
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="root"></param>
        public Dictionary<int, Hexagon> CreateMap(HexagonMapPlugin[] hexagons, GameObject root)
        {
            Dictionary<int, Hexagon> hexagonDic = new Dictionary<int, Hexagon>();
            for (int i = 0; i < hexagons.Length; i++)
            {
                if (hexagonDic.ContainsKey(hexagons[i].ID))
                    continue;

                var hexagon = Factory.Instance.GetHexagon(hexagons[i]);
                hexagon.SetParent(root.transform);
                hexagonDic[hexagons[i].ID] = hexagon;
            }
            return hexagonDic;
        }

        //public Dictionary<int, Bonfire> CreateBonfire(BonfireMapPlugin[] bonfires, GameObject root)
        //{
        //    Dictionary<int, Bonfire> bonfireDic = new Dictionary<int, Bonfire>();
        //    if (null != bonfires)
        //    {
        //        for (int i = 0; i < bonfires.Length; i++)
        //        {
        //            var bonfire = new Bonfire(bonfires[i].ID, bonfires[i].configId, bonfires[i].hexagonID);
        //            bonfire.SetParent(root.transform);
        //            bonfireDic[bonfires[i].ID] = bonfire;
        //        }
        //    }
        //    return bonfireDic;
        //}

        public Dictionary<int, Ornament> CreateOrnament(OrnamentMapPlugin[] ornaments, GameObject root)
        {
            Dictionary<int, Ornament> ornamentsDic = new Dictionary<int, Ornament>();
            if (null != ornaments)
            {
                for (int i = 0; i < ornaments.Length; i++)
                {
                    var ornament = Factory.Instance.GetOrnament(ornaments[i]);
                    ornament.SetParent(root.transform);
                    ornamentsDic[ornaments[i].ID] = ornament;
                }
            }
            return ornamentsDic;
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
                var hexagonCell = new HexagonMapPlugin(GetHexagonKey(coor), data.ConfigId, data.IsReachable, coor);
                hexagons[i] = hexagonCell;
            }

            var roleRootObj = GameObject.Find("RoleRoot");
            var enemyCount = roleRootObj.transform.childCount;
            NewEnemyMapPlugin[] enemys = new NewEnemyMapPlugin[enemyCount];
            for (int i = 0; i < enemyCount; i++)
            {
                var enemyTra = roleRootObj.transform.GetChild(i);
                var data = enemyTra.GetComponent<RoleBehaviour>();

                //var vec = GetCoorFromPos(enemyTra.position - CommonParams.Offset);
                //DebugManager.Instance.Log(enemyTra.position - CommonParams.Offset);
                //DebugManager.Instance.Log(vec.x + "/" + vec.y + "/" + vec.z);
                //DebugManager.Instance.Log(GetHexagonKey(GetCoorFromPos(enemyTra.position - CommonParams.Offset)));
                enemys[i] = new NewEnemyMapPlugin(data.ID, GetHexagonKey(GetCoorFromPos(enemyTra.position - CommonParams.Offset)));
            }

            var fireRootObj = GameObject.Find("BonfireRoot");
            var bonfireCount = fireRootObj.transform.childCount;
            BonfireMapPlugin[] bonfires = new BonfireMapPlugin[bonfireCount];
            for (int i = 0; i < bonfireCount; i++)
            {
                var bonfireTra = fireRootObj.transform.GetChild(i);
                var data = bonfireTra.GetComponent<BonfireBehaviour>();
                var hexagonID = GetHexagonKey(GetCoorFromPos(bonfireTra.position - CommonParams.Offset));
                bonfires[i] = new BonfireMapPlugin(hexagonID, data.ConfigID, hexagonID);
            }

            var ornamentRootObj = GameObject.Find("OrnamentRoot");
            var ornamentCount = ornamentRootObj.transform.childCount;
            OrnamentMapPlugin[] ornaments = new OrnamentMapPlugin[ornamentCount];
            for (int i = 0; i < ornamentCount; i++)
            {
                var ornamentTra = ornamentRootObj.transform.GetChild(i);
                var data = ornamentTra.GetComponent<OrnamentBehaviour>();
                var hexagonID = GetHexagonKey(GetCoorFromPos(ornamentTra.position - CommonParams.Offset));
                var eulerAngles = ornamentTra.localEulerAngles;
                var rotation = new WGVector3(eulerAngles.x, eulerAngles.y, eulerAngles.z);
                ornaments[i] = new OrnamentMapPlugin(hexagonID, data.ConfigID, hexagonID, ornamentTra.localScale.x, rotation);
            }

            var skyBox = GameObject.Find("Main Camera").GetComponent<Skybox>();
            var mainLight = GameObject.Find("Directional Light").GetComponent<Light>();
            var lightingPlugin = new LightingPlugin(skyBox.material.name, mainLight.color);

            var points = GameObject.Find("FloatPoint").GetComponent<MapFloatPoint>().Points;
            var levelPlugin = new LevelMapPlugin(hexagons, enemys, bonfires, ornaments, lightingPlugin, points);

            var dir = EditorUtility.SaveFilePanel("导出地图", Application.dataPath + "/Maps", "地图", "json");
            if (null == dir || dir.Equals(""))
                return;

            Tool.Instance.WriteJson<LevelMapPlugin>(dir, levelPlugin);
        }

        /// <summary>
        /// 打开地图的接口
        /// </summary>
        public void OpenEditorMap()
        {
            if (!IsActiveMapEditor())
                return;

            var dir = EditorUtility.OpenFilePanel("打开地图", Application.streamingAssetsPath + "/Maps", "json");
            if (null == dir || "" == dir)
                return;

            if (!File.Exists(dir))
                return;

            ClearEditorMapScene();

            LevelMapPlugin levelPlugin = Tool.Instance.ReadJson<LevelMapPlugin>(dir);

            MapManager.Instance.CreateMap(levelPlugin.hexagons, levelPlugin.bonfires, levelPlugin.ornaments, levelPlugin.lightingPlugin);

            MapManager.Instance.InitBonfires(levelPlugin.bonfires);

            RoleManager.Instance.InitLevelRoles(levelPlugin.enemys);

            GameObject.Find("FloatPoint").GetComponent<MapFloatPoint>().Points = levelPlugin.points;
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
                            obj.transform.position = MapTool.Instance.GetPosFromCoor(new WGVector3(i, q, j));
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

            var ornamentRootObj = GameObject.Find("OrnamentRoot");
            var ornamentCount = ornamentRootObj.transform.childCount;
            for (int i = ornamentCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(ornamentRootObj.transform.GetChild(i).gameObject);
            }
        }
#endif

        /// <summary>
        /// 获取地块在构建的地图数据中的key
        /// </summary>
        /// <returns></returns>
        public int GetHexagonKey(WGVector3 pos)
        {
            var key = (int)pos.x;
            key <<= 8;
            key += (int)pos.y;
            key <<= 8;
            key += (int)pos.z;
            return key;

            //    0,-3, 0
            //    00000000 00000000 00000000 00000000
            //    +
            //    11111111 11111111 11111111 11111100 负数以补码形式存在
            //    ||
            //    11111111 11111111 11111111 11111100
            //    <<8
            //    ||
            //    11111111 11111111 11111100 00000000
            //    ||转原码
            //    10000000 00000000 00000011 00000000
        }
    }
}
