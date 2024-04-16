//using UnityEngine;
//using UnityEditor;

//public class CustomMoveTool : EditorWindow
//{
//    private Vector3 customMoveAxis = Vector3.forward; // �����Զ���λ����ķ���
//    private Color customMoveAxisColor = Color.yellow; // �����Զ���λ�������ɫ
//    private bool customMoveEnabled = true; // �Ƿ������Զ���λ����
//    private float moveAmount = 1f; // ����λ����

//    [MenuItem("Window/Custom Move Tool")]
//    public static void ShowWindow()
//    {
//        EditorWindow.GetWindow(typeof(CustomMoveTool));
//    }

//    private void OnGUI()
//    {
//        customMoveEnabled = EditorGUILayout.Toggle("Enable Custom Move Tool", customMoveEnabled);
//        customMoveAxis = EditorGUILayout.Vector3Field("Custom Move Axis", customMoveAxis);
//        customMoveAxisColor = EditorGUILayout.ColorField("Custom Move Axis Color", customMoveAxisColor);
//        moveAmount = EditorGUILayout.FloatField("Move Amount", moveAmount);
//    }

//    private void OnSceneGUI(SceneView sceneView)
//    {
//        if (!customMoveEnabled)
//            return;

//        Handles.color = customMoveAxisColor;
//        Vector3 moveVector = customMoveAxis.normalized * moveAmount;

//        // �ڳ�����ͼ�л����Զ���λ����
//        Handles.ArrowHandleCap(
//            GUIUtility.GetControlID(FocusType.Passive),
//            Vector3.zero,
//            Quaternion.LookRotation(customMoveAxis),
//            HandleUtility.GetHandleSize(Vector3.zero),
//            EventType.Repaint
//        );

//        // �����û����벢ִ��λ�Ʋ���
//        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
//        {
//            Event.current.Use();

//            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
//            RaycastHit hitInfo;

//            if (Physics.Raycast(ray, out hitInfo))
//            {
//                Undo.RecordObject(hitInfo.collider.gameObject.transform, "Custom Move");

//                Vector3 newPosition = hitInfo.collider.gameObject.transform.position + moveVector;
//                hitInfo.collider.gameObject.transform.position = newPosition;
//            }
//        }
//    }
//}
