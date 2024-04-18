using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class BuildMap : EditorWindow
{
    private const string ToolTitle = "��ͼ�༭��";
    private const string MapPath = "Scenes/MapEditorScene";
    /// <summary>
    /// �򿪵�ͼ�༭��
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

        //�򿪵�ͼ�༭����
        if (GUILayout.Button("�򿪵�ͼ�༭����"))
        {
            OpenMapScene();
        }

        EditorGUILayout.Space();

        //��յ�ͼ�༭����
        if (GUILayout.Button("��յ�ͼ�༭����"))
        {
            ClearMapScene();
        }

        EditorGUILayout.Space();

        //������ͼ
        if (GUILayout.Button("������ͼ"))
        {
            SaveMap();
        }

        EditorGUILayout.Space();

        // �򿪵�ͼ
        GUILayout.Label("��ǰѡ�е�ͼ��");
        if (GUILayout.Button("�򿪵�ͼ"))
        {
            OpenMap();
        }
    }

    /// <summary>
    /// �򿪵�ͼ�༭ר�õĳ���
    /// </summary>
    private void OpenMapScene()
    {
        EditorSceneManager.OpenScene(MapPath);
    }

    /// <summary>
    /// ������ͼ�Ľӿ�
    /// </summary>
    private void SaveMap()
    {
        var dir = EditorUtility.SaveFilePanel("������ͼ", Application.dataPath + "/Maps", "��ͼ", "json");

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
    /// �򿪵�ͼ�Ľӿ�
    /// </summary>
    private void OpenMap()
    {
        //Undo.RecordObject(transform, "Open Map");
        ClearMapScene();

        var dir = EditorUtility.OpenFilePanel("�򿪵�ͼ", Application.dataPath + "/Maps", "");
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
    /// ��ճ���
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
