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
        //��ͼ�༭����·��
        private const string MapPath = "Assets/Scenes/MapEditorScene.unity";

        //�������ھ�
        private float _insideDiameter = 1.0f;

        //z��ƫ�ƻ���
        private const float _radian = 30.0F / 180.0F * Mathf.PI;

        //�߶�
        private const float _height = 0.23F;


        /// <summary>
        /// ����������ת���ɵ�ͼ��������
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
        /// �ӵ�ͼ�������ת������������
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
        /// ������ͼ
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="root"></param>
        public Dictionary<string, Hexagon> CreateMap(HexagonMapPlugin[] hexagons, GameObject root)
        {
            Dictionary<string, Hexagon> hexagonDic = new Dictionary<string, Hexagon>();
            for (int i = 0; i < hexagons.Length; i++)
            {
                var hexagon = new Hexagon(hexagons[i].ID, hexagons[i].configId, hexagons[i].coor);
                hexagon.SetParent(root.transform);
                hexagonDic[GetHexagonKey(hexagon.coor)] = hexagon;
            }
            return hexagonDic;
        }

#if UNITY_EDITOR

        /// <summary>
        /// �Ƿ�����ͼ�༭ģʽ
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
        /// �򿪵�ͼ�༭ר�õĳ���
        /// </summary>
        public void OpenEditorMapScene()
        {
            if (Application.isPlaying)
                return;

            EditorSceneManager.OpenScene(MapPath);
        }

        /// <summary>
        /// ������ͼ�Ľӿ�
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
                var hexagonCell = new HexagonMapPlugin(GetHexagonKey(coor), data.configId, coor);
                //DebugManager.Instance.Log(coor);
                hexagons[i] = hexagonCell;
            }

            var roleRootObj = GameObject.Find("RoleRoot");
            var enemyCount = roleRootObj.transform.childCount;
            EnemyMapPlugin[] enemys = new EnemyMapPlugin[enemyCount];
            for (int i = 0; i < enemyCount; i++)
            {
                var enemyTra = roleRootObj.transform.GetChild(i);
                var data = enemyTra.GetComponent<RoleBehaviour>();
                enemys[i] = new EnemyMapPlugin(data.ID, GetHexagonKey(GetCoorFromPos(enemyTra.position)));
            }

            var levelPlugin = new LevelMapPlugin(hexagons, enemys);

            var dir = EditorUtility.SaveFilePanel("������ͼ", Application.dataPath + "/Maps", "��ͼ", "json");
            Tool.Instance.WriteJson<LevelMapPlugin>(dir, levelPlugin);
        }

        /// <summary>
        /// �򿪵�ͼ�Ľӿ�
        /// </summary>
        public void OpenEditorMap()
        {
            if (!IsActiveMapEditor())
                return;

            ClearEditorMapScene();

            var dir = EditorUtility.OpenFilePanel("�򿪵�ͼ", Application.dataPath + "/Maps", "");
            LevelMapPlugin levelPlugin = Tool.Instance.ReadJson<LevelMapPlugin>(dir);

            var rootObj = GameObject.Find("Root");
            MapManager.Instance.CreateMap(levelPlugin.hexagons);
            //for (int i = 0; i < levelPlugin.hexagons.Length; i++)
            //{
            //    var hexagon = new Hexagon(levelPlugin.hexagons[i].ID, levelPlugin.hexagons[i].configId, levelPlugin.hexagons[i].coor);
            //    //hexagon.CreateGO();
            //    hexagon.SetParent(rootObj.transform);
            //}

            var enemyRootObj = GameObject.Find("RoleRoot");

            RoleManager.Instance.InitEnemys(levelPlugin.enemys);
            //for (int i = 0; i < levelPlugin.enemys.Length; i++)
            //{
            //    var enemyConfig = ConfigMgr.Instance.GetConfig<LevelEnemyConfig>("LevelEnemyConfig", levelPlugin.enemys[i].configId);
            //    Debug.Log(enemyConfig.ID);
            //    var enemy = new Enemy(new LevelRoleData(enemyConfig.ID, enemyConfig.RoleID, enemyConfig.Level, enemyConfig.EquipDic, null));
            //    enemy.SetParent(enemyRootObj.transform);
            //}
        }

        /// <summary>
        /// �������ɵ�ͼ
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
                        Enum.HexagonType type = Enum.HexagonType.BeachShore;
                        foreach (var pair in castDic)
                        {
                            if (rd < pair.Key && pair.Key <= minKey)
                            {
                                minKey = pair.Key;
                                type = pair.Value;
                            }
                        }
                        Debug.Log(type);
                        string assetPath = ConfigMgr.Instance.GetConfig<HexagonConfig>("HexagonConfig", (int)type).Prefab;
                        AssetMgr.Instance.LoadAssetAsync<GameObject>(assetPath, (GameObject prefab) =>
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
        /// ��ճ���
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
        }
#endif
        /// <summary>
        /// ��ȡ�ؿ��ڹ����ĵ�ͼ�����е�key
        /// </summary>
        /// <returns></returns>
        public string GetHexagonKey(Vector3 pos)
        {
            return pos.x + "_" + pos.y + "_" + pos.z;
        }
    }
}
