using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class BuildMap : EditorWindow
{
    private const string ToolTitle = "地图编辑器";
    private const string MapPath = "Scenes/MapEditorScene";
    /// <summary>
    /// 打开地图编辑器
    /// </summary>
    [MenuItem("Tools/" + ToolTitle)]
    public static void OpenMapEditor()
    {
        var wd = GetWindow<BuildMap>();
        wd.titleContent = new GUIContent(ToolTitle);
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
    }

    /// <summary>
    /// 打开地图编辑专用的场景
    /// </summary>
    private void OpenMapScene()
    {
        EditorSceneManager.OpenScene(MapPath);
    }

    /// <summary>
    /// 导出地图的接口
    /// </summary>
    private void SaveMap()
    {
        var dir = EditorUtility.SaveFilePanel("导出地图", Application.dataPath + "/Maps", "地图", "json");

        var rootObj = GameObject.Find("Root");
        var hexagonCount = rootObj.transform.childCount;
        HexagonCell[] hexagons = new HexagonCell[hexagonCount];

        for (int i = 0; i < hexagonCount; i++)
        {
            var hexagonTra = rootObj.transform.GetChild(i);
            var data = hexagonTra.GetComponent<HexagonCellData>();
            Debug.Log(data.config.type);
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
        //Undo.RecordObject(transform, "Open Map");
        ClearMapScene();

        var dir = EditorUtility.OpenFilePanel("打开地图", Application.dataPath + "/Maps", "");
        var jsonStr = File.ReadAllText(dir);

        var rootObj = GameObject.Find("Root");
        HexagonCell[] hexagons = WarGame.Tool.Instance.FromJson<HexagonCell[]>(jsonStr);
        Dictionary<Enum.HexagonType, GameObject> prefabDic = new Dictionary<Enum.HexagonType, GameObject>();

        for (int i = 0; i < hexagons.Length; i++)
        {
            var hexagon = hexagons[i];
            string assetPath = "";
            GameObject prefab = null;
            if (!prefabDic.TryGetValue(hexagon.config.type, out prefab))
            {
                switch (hexagon.config.type)
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
                Debug.Log(assetPath);

                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                prefabDic[hexagon.config.type] = prefab;
            }
            var obj = Instantiate(prefab);
            obj.transform.position = MapTool.Instance.FromCellPosToWorldPos(hexagon.position);
            obj.transform.SetParent(rootObj.transform);
        }
    }

    /// <summary>
    /// 清空场景
    /// </summary>
    private void ClearMapScene()
    {
        var rootObj = GameObject.Find("Root");
        var hexagonCount = rootObj.transform.childCount;

        for (int i = hexagonCount - 1; i >= 0; i--)
        {
            DestroyImmediate(rootObj.transform.GetChild(i).gameObject);
        }
    }
}
