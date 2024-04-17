using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace WarGame
{
    public class BuildMap : EditorWindow
    {
        private const string ToolTitle = "地图编辑器";
        private const string MapPath = "Assets/Scenes/MapEditorScene.unity";
        private static Dictionary<Enum.HexagonType, int> weights = new Dictionary<Enum.HexagonType, int>();
        private static int xNum, yNum, zNum;

        /// <summary>
        /// 打开地图编辑器
        /// </summary>
        [MenuItem("Tools/" + ToolTitle)]
        public static void OpenMapEditor()
        {
            var wd = GetWindow<BuildMap>();
            wd.titleContent = new GUIContent(ToolTitle);

            foreach (var v in System.Enum.GetValues(typeof(Enum.HexagonType)))
            {
                weights[(Enum.HexagonType)v] = 1;
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();

            //打开地图编辑场景
            if (GUILayout.Button("打开地图编辑场景"))
            {
                OpenMapScene();
            }

            EditorGUILayout.Space();

            //清空地图编辑场景
            if (GUILayout.Button("清空地图编辑场景"))
            {
                ClearMapScene();
            }

            EditorGUILayout.Space();

            //导出地图
            if (GUILayout.Button("导出地图"))
            {
                SaveMap();
            }

            EditorGUILayout.Space();

            // 打开地图
            GUILayout.Label("当前选中地图：");
            if (GUILayout.Button("打开地图"))
            {
                OpenMap();
            }

            EditorGUILayout.Space();

            // 快速生成地图
            GUILayout.Label("快速生成地图：");
            GUILayout.Label("X数量：");
            int.TryParse(GUILayout.TextField(xNum.ToString()), out xNum);
            GUILayout.Label("Y数量：");
            int.TryParse(GUILayout.TextField(yNum.ToString()), out yNum);
            GUILayout.Label("Z数量：");
            int.TryParse(GUILayout.TextField(zNum.ToString()), out zNum);

            foreach (var v in System.Enum.GetValues(typeof(Enum.HexagonType)))
            {
                var type = (Enum.HexagonType)v;
                GUILayout.Label(type + " 权重：");
                int a = 0;
                int.TryParse(GUILayout.TextField(weights[type].ToString()), out a);
                weights[type] = a;
            }
            if (GUILayout.Button("打开地图"))
            {
                QuickGenerageMap(xNum, yNum, zNum, weights);
            }
        }

        /// <summary>
        /// 打开地图编辑专用的场景
        /// </summary>
        private void OpenMapScene()
        {
            if (Application.isPlaying)
                return;

            EditorSceneManager.OpenScene(MapPath);
        }

        /// <summary>
        /// 导出地图的接口
        /// </summary>
        private void SaveMap()
        {
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            var dir = EditorUtility.SaveFilePanel("导出地图", Application.dataPath + "/Maps", "地图", "json");

            var rootObj = GameObject.Find("Root");
            var hexagonCount = rootObj.transform.childCount;
            HexagonCell[] hexagons = new HexagonCell[hexagonCount];

            for (int i = 0; i < hexagonCount; i++)
            {
                var hexagonTra = rootObj.transform.GetChild(i);
                var data = hexagonTra.GetComponent<HexagonCellData>();
                var hexagonCell = new HexagonCell(i, data.config);
                hexagonCell.position = MapTool.Instance.FromWorldPosToCellPos(hexagonTra.position);
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
        private void OpenMap()
        {
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            ClearMapScene();

            var dir = EditorUtility.OpenFilePanel("打开地图", Application.dataPath + "/Maps", "");
            var jsonStr = File.ReadAllText(dir);

            var rootObj = GameObject.Find("Root");
            HexagonCell[] hexagons = WarGame.Tool.Instance.FromJson<HexagonCell[]>(jsonStr);

            for (int i = 0; i < hexagons.Length; i++)
            {
                var hexagon = hexagons[i];
                string assetPath = MapTool.Instance.GetHexagonPrefab(hexagon.config.type);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                var obj = Instantiate(prefab);
                obj.transform.position = MapTool.Instance.FromCellPosToWorldPos(hexagon.position);
                obj.transform.SetParent(rootObj.transform);
            }
        }

        //快速生成地图
        private void QuickGenerageMap(int xNum, int yNum, int zNum, Dictionary<Enum.HexagonType, int> weightList)
        {
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            ClearMapScene();

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
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        var obj = Instantiate(prefab);
                        obj.transform.position = MapTool.Instance.FromCellPosToWorldPos(new Vector3(i, q, j));
                        obj.transform.SetParent(rootObj.transform);
                    }
                }
            }
        }

        /// <summary>
        /// 清空场景
        /// </summary>
        private void ClearMapScene()
        {
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            var rootObj = GameObject.Find("Root");
            var hexagonCount = rootObj.transform.childCount;

            for (int i = hexagonCount - 1; i >= 0; i--)
            {
                DestroyImmediate(rootObj.transform.GetChild(i).gameObject);
            }
        }
    }
}
