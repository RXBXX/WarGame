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
        private const string ToolTitle = "地图编辑器";
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
                MapTool.Instance.OpenEditorMapScene();
            }

            EditorGUILayout.Space();

            //清空地图编辑场景
            if (GUILayout.Button("清空地图编辑场景"))
            {
                MapTool.Instance.ClearEditorMapScene();
            }

            EditorGUILayout.Space();

            //导出地图
            if (GUILayout.Button("导出地图"))
            {
                MapTool.Instance.SaveEditorMap();
            }

            EditorGUILayout.Space();

            // 打开地图
            GUILayout.Label("当前选中地图：");
            if (GUILayout.Button("打开地图"))
            {
                MapTool.Instance.OpenEditorMap();
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
                GUILayout.BeginHorizontal();
                var type = (Enum.HexagonType)v;
                GUILayout.Label(type + " 权重：");
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
            if (GUILayout.Button("快速生成地图"))
            {
                MapTool.Instance.QuickGenerageEditorMap(xNum, yNum, zNum, weights);
            }
        }
    }
#endif
}
