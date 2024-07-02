using UnityEngine;
using UnityEditor;

namespace WarGame
{
    [CustomEditor(typeof(Transform))]
    public class CustomTransformEditor : Editor
    {
        private bool customAxesEnabled = true;

        //�Զ�������
        private Vector3[] customAxes = new Vector3[3]; // �޸�Ϊ����Ҫ��������

        //��ǰѡ�е���ק��
        private int dragingAxis = -1;

        private Vector2 mousePos = Vector2.zero;

        private Vector3 tempPos = Vector2.zero;


        ///��C#�У����캯����Ҳ��Ϊʵ�����캯���������ڳ�ʼ�����ʵ�������ⷽ�������캯������������Ϊ��̬�ģ���static������Ϊ��̬��Ա�������౾����������ʵ����
        ///��˹��캯�������Ǿ�̬�ġ�
        ///�����ϣ��ִ��һЩ��ʼ���߼�������ϣ�����߼������һ�α�ʹ��ʱִ�У�����Կ���ʹ�þ�̬���캯����static constructor����
        ///��̬���캯�������ڳ�ʼ����ľ�̬��Ա�ģ��������һ�α�ʹ��ʱ�Զ����ã�����ֻ�ᱻ����һ�Ρ���̬���캯�����ܴ��з������η���Ҳ���ܽ����κβ�����
        static CustomTransformEditor()
        {
            // ע�᳡���仯�¼�
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

        //���ڳ�����ѡ�ж�����Inspector�����ʾʱ�����������ӿ�
        public override void OnInspectorGUI()
        {
            Transform transform = (Transform)target;

            // ��ʾλ�ú�����
            EditorGUI.BeginChangeCheck();
            Vector3 position = EditorGUILayout.Vector3Field("Position", transform.localPosition);
            Vector3 scale = EditorGUILayout.Vector3Field("Scale", transform.localScale);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(transform, "Change Transform");
                transform.localPosition = position;
                transform.localScale = scale;
            }

            // ����תת��Ϊŷ����
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

        //���ڳ�����ѡ�ж���ʱ�����������ӿ�
        private void OnSceneGUI()
        {
            //Debug.Log("OnSceneGUI()");
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            if (!customAxesEnabled)
                return;

            // ���ر༭��ͼ��Ĭ�ϵĲ�����
            // ��ȡ��ǰScene��ͼ�еĹ���
            UnityEditor.Tool currentTool = UnityEditor.Tools.current;
            // ���ù���
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

                // �ڳ�����ͼ�л����Զ���λ����
                Handles.ArrowHandleCap(
                    GUIUtility.GetControlID(FocusType.Passive),
                    transform.position,
                    Quaternion.LookRotation(worldAxis),
                    HandleUtility.GetHandleSize(Vector3.zero) + 0.1F,
                    EventType.Repaint
                );

                // ��ȡλ����������յ�
                Vector3 startPosition = transform.position;
                Vector3 endPosition = transform.position + moveVector * HandleUtility.GetHandleSize(Vector3.zero);

                // �������¼�
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
                            //������϶�ӳ�䵽�� 3D �ռ���һ��ֱ�ߵ��˶���
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

        //�����������������У���ӵ�Root�ڵ���
        private static void OnHierarchyChanged()
        {
            if (!MapTool.Instance.IsActiveMapEditor())
                return;

            // ��ȡ�����е����еؿ�
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