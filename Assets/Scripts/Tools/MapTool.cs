using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;

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


        //public float _insideDiameter
        //{
        //    get { return _insideDiameter; }
        //}

        //public float _radian
        //{
        //    get { return _radian; }
        //}

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

        ///��ȡ����Ԥ����
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

            var dir = EditorUtility.SaveFilePanel("������ͼ", Application.dataPath + "/Maps", "��ͼ", "json");

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
        /// �򿪵�ͼ�Ľӿ�
        /// </summary>
        public void OpenEditorMap()
        {
            if (!IsActiveMapEditor())
                return;

            ClearEditorMapScene();

            var dir = EditorUtility.OpenFilePanel("�򿪵�ͼ", Application.dataPath + "/Maps", "");
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
        }

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
