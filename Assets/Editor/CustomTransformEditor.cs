using UnityEngine;
using UnityEditor;

namespace WarGame
{
    [CustomEditor(typeof(Transform))]
    public class CustomTransformEditor : Editor
    {
        private bool customAxesEnabled = true;

        //自定义轴向
        private Vector3[] customAxes = new Vector3[3]; // 修改为你想要的轴数量

        //当前选中的拖拽轴
        private int dragingAxis = -1;

        private Vector2 mousePos = Vector2.zero;

        private Vector3 tempPos = Vector2.zero;


        ///在C#中，构造函数（也称为实例构造函数）是用于初始化类的实例的特殊方法。构造函数不允许声明为静态的（即static），因为静态成员是属于类本身而不是类的实例，
        ///因此构造函数不能是静态的。
        ///如果你希望执行一些初始化逻辑，并且希望该逻辑在类第一次被使用时执行，你可以考虑使用静态构造函数（static constructor）。
        ///静态构造函数是用于初始化类的静态成员的，它在类第一次被使用时自动调用，并且只会被调用一次。静态构造函数不能带有访问修饰符，也不能接受任何参数。
        static CustomTransformEditor()
        {
            // 注册场景变化事件
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void OnEnable()
        {
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            customAxes[0] = new Vector3(1, 0, 0);
            customAxes[1] = new Vector3(Mathf.Sin(30.0f / 180.0f * Mathf.PI), 0, Mathf.Cos(30.0f / 180.0f * Mathf.PI));
            customAxes[2] = Vector3.up;
        }

        //当在场景中选中对象且Inspector面板显示时，会调用这个接口
        public override void OnInspectorGUI()
        {
            Transform transform = (Transform)target;

            // 显示位置和缩放
            EditorGUI.BeginChangeCheck();
            Vector3 position = EditorGUILayout.Vector3Field("Position", transform.localPosition);
            Vector3 scale = EditorGUILayout.Vector3Field("Scale", transform.localScale);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(transform, "Change Transform");
                transform.localPosition = position;
                transform.localScale = scale;
            }

            // 将旋转转换为欧拉角
            EditorGUI.BeginChangeCheck();
            Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", transform.localEulerAngles);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(transform, "Change Rotation");
                transform.localEulerAngles = rotation;
            }

            //base.OnInspectorGUI();

            //if (!MapTool.Instance.IsActiveMapEditor())
            //    return;

            //EditorGUILayout.Space();
            //EditorGUILayout.LabelField("Custom Axes", EditorStyles.boldLabel);
            //customAxesEnabled = EditorGUILayout.Toggle("Enable Custom Axes", customAxesEnabled);

            //if (customAxesEnabled)
            //{
            //    EditorGUI.indentLevel++;

            //    for (int i = 0; i < customAxes.Length; i++)
            //    {
            //        EditorGUI.BeginChangeCheck();
            //        Vector3 customAxis = EditorGUILayout.Vector3Field("Axis " + i, customAxes[i]);
            //        if (EditorGUI.EndChangeCheck())
            //        {
            //            Undo.RecordObject(transform, "Change Custom Axes");
            //            customAxes[i] = customAxis;
            //        }
            //    }

            //    EditorGUI.indentLevel--;
            //}
        }

        //当在场景中选中对象时，会调用这个接口
        private void OnSceneGUI()
        {
            //Debug.Log("OnSceneGUI()");
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            if (!customAxesEnabled)
                return;

            // 隐藏编辑视图下默认的操作轴
            // 获取当前Scene视图中的工具
            UnityEditor.Tool currentTool = UnityEditor.Tools.current;
            // 重置工具
            if ((currentTool == UnityEditor.Tool.Move || currentTool == UnityEditor.Tool.Rect))
            {
                Tools.current = UnityEditor.Tool.None;
            }
            if (currentTool == UnityEditor.Tool.Rotate)
            {
                Tools.current = UnityEditor.Tool.None;
            }
            if (currentTool == UnityEditor.Tool.Scale)
            {
                Tools.current = UnityEditor.Tool.None;
            }

            Transform transform = (Transform)target;
            //Debug.Log(target.name);
            for (int i = 0; i < customAxes.Length; i++)
            {
                Handles.color = GetAxisColor(i);
                Vector3 worldAxis = transform.TransformDirection(customAxes[i]);
                Vector3 moveVector = worldAxis.normalized;

                // 在场景视图中绘制自定义位移轴
                Handles.ArrowHandleCap(
                    GUIUtility.GetControlID(FocusType.Passive),
                    transform.position,
                    Quaternion.LookRotation(worldAxis),
                    HandleUtility.GetHandleSize(Vector3.zero) + 0.1F,
                    EventType.Repaint
                );

                // 获取位移轴的起点和终点
                Vector3 startPosition = transform.position;
                Vector3 endPosition = transform.position + moveVector * HandleUtility.GetHandleSize(Vector3.zero);

                // 检测鼠标事件
                Event guiEvent = Event.current;
                //Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
                switch (guiEvent.type)
                {
                    case EventType.MouseDown:
                        Debug.Log("MouseDown");
                        if (guiEvent.button == 0 && HandleUtility.DistanceToLine(endPosition, startPosition) < 10f)
                        {
                            dragingAxis = i;
                            mousePos = guiEvent.mousePosition;
                            tempPos = transform.position;
                            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                            guiEvent.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (dragingAxis >= 0)
                        {
                            Undo.RecordObject(transform, "Move Object");

                            var oldPos = transform.position;
                            //将鼠标拖动映射到沿 3D 空间中一条直线的运动。
                            Vector3 tempWorldAxis = transform.TransformDirection(customAxes[dragingAxis]);
                            Vector3 tempMoveVector = tempWorldAxis.normalized;
                            float delta = HandleUtility.CalcLineTranslation(mousePos, guiEvent.mousePosition, oldPos, tempMoveVector);
                            Debug.Log(tempPos);
                            Vector3 newPos = tempMoveVector * delta + tempPos;

                            var hexMapPos = MapTool.Instance.GetCoorFromPos(newPos);
                            newPos = MapTool.Instance.GetPosFromCoor(hexMapPos);

                            var posDirty = false;
                            if (0 == dragingAxis && newPos.x != oldPos.x)
                            {
                                posDirty = true;
                            }
                            else if (1 == dragingAxis && newPos.x != oldPos.x && newPos.z != oldPos.z)
                            {
                                posDirty = true;
                            }
                            else if (2 == dragingAxis && newPos.y != oldPos.y)
                            {
                                posDirty = true;
                            }

                            if (posDirty)
                            {
                                transform.position = newPos;
                            }

                            HandleUtility.Repaint();
                            guiEvent.Use();
                        }
                        break;
                    case EventType.MouseUp:
                        if (dragingAxis >= 0)
                        {
                            dragingAxis = -1;
                            mousePos = Vector2.zero;
                            GUIUtility.hotControl = 0;
                            guiEvent.Use();
                        }
                        break;
                }
            }
        }

        private Color GetAxisColor(int index)
        {
            switch (index)
            {
                case 0:
                    return Color.red;
                case 1:
                    return Color.green;
                case 2:
                    return Color.blue;
                default:
                    return Color.white;
            }
        }

        //当对象被新增到场景中，添加到Root节点下
        private static void OnHierarchyChanged()
        {
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            // 获取场景中的所有地块
            var rootObj = GameObject.Find("Root");
            GameObject[] rootObjects = GameObject.FindGameObjectsWithTag(Enum.Tag.Hexagon.ToString());
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].transform.parent != rootObj)
                {
                    rootObjects[i].transform.SetParent(rootObj.transform);
                }
            }

            var roleRootObj = GameObject.Find("RoleRoot");
            GameObject[] enemys = GameObject.FindGameObjectsWithTag(Enum.Tag.Enemy.ToString());
            for (int i = 0; i < enemys.Length; i++)
            {
                if (enemys[i].transform.parent != roleRootObj)
                {
                    enemys[i].transform.SetParent(roleRootObj.transform);
                }
            }

            var fireRootObj = GameObject.Find("BonfireRoot");
            GameObject[] bonfires = GameObject.FindGameObjectsWithTag(Enum.Tag.Bonfire.ToString());
            for (int i = 0; i < bonfires.Length; i++)
            {
                if (bonfires[i].transform.parent != fireRootObj)
                {
                    bonfires[i].transform.SetParent(fireRootObj.transform);
                }
            }

            var ornamentRootObj = GameObject.Find("OrnamentRoot");
            GameObject[] ornaments = GameObject.FindGameObjectsWithTag(Enum.Tag.Ornament.ToString());
            for (int i = 0; i < ornaments.Length; i++)
            {
                if (ornaments[i].transform.parent != ornamentRootObj)
                {
                    ornaments[i].transform.SetParent(ornamentRootObj.transform);
                }
            }
        }
    }
}