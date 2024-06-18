using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace WarGame
{
#if UNITY_EDITOR
    public class BuildMap : EditorWindow
    {
        private const string ToolTitle = "��ͼ�༭��";
        private static Dictionary<Enum.HexagonType, int> weights = new Dictionary<Enum.HexagonType, int>();
        private static int xNum, yNum, zNum;

        /// <summary>
        /// �򿪵�ͼ�༭��
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

            //�򿪵�ͼ�༭����
            if (GUILayout.Button("�򿪵�ͼ�༭����"))
            {
                MapTool.Instance.OpenEditorMapScene();
            }

            EditorGUILayout.Space();

            //��յ�ͼ�༭����
            if (GUILayout.Button("��յ�ͼ�༭����"))
            {
                MapTool.Instance.ClearEditorMapScene();
            }

            EditorGUILayout.Space();

            //������ͼ
            if (GUILayout.Button("������ͼ"))
            {
                MapTool.Instance.SaveEditorMap();
            }

            EditorGUILayout.Space();

            // �򿪵�ͼ
            GUILayout.Label("��ǰѡ�е�ͼ��");
            if (GUILayout.Button("�򿪵�ͼ"))
            {
                MapTool.Instance.OpenEditorMap();
            }

            EditorGUILayout.Space();

            // �������ɵ�ͼ
            GUILayout.Label("�������ɵ�ͼ��");
            GUILayout.Label("X������");
            int.TryParse(GUILayout.TextField(xNum.ToString()), out xNum);
            GUILayout.Label("Y������");
            int.TryParse(GUILayout.TextField(yNum.ToString()), out yNum);
            GUILayout.Label("Z������");
            int.TryParse(GUILayout.TextField(zNum.ToString()), out zNum);

            foreach (var v in System.Enum.GetValues(typeof(Enum.HexagonType)))
            {
                GUILayout.BeginHorizontal();
                var type = (Enum.HexagonType)v;
                GUILayout.Label(type + " Ȩ�أ�");
                int a = 0;
                string weight = "0";
                if (weights.ContainsKey(type))
                {
                    weight = weights[type].ToString();
                }
                int.TryParse(GUILayout.TextField(weight), out a);
                weights[type] = a;
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("�������ɵ�ͼ"))
            {
                MapTool.Instance.QuickGenerageEditorMap(xNum, yNum, zNum, weights);
            }
        }
    }
#endif
}
