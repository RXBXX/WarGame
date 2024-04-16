//using UnityEngine;
//using UnityEditor;

//public class CustomMoveTool : EditorWindow
//{
//    private Vector3 customMoveAxis = Vector3.forward; // 设置自定义位移轴的方向
//    private Color customMoveAxisColor = Color.yellow; // 设置自定义位移轴的颜色
//    private bool customMoveEnabled = true; // 是否启用自定义位移轴
//    private float moveAmount = 1f; // 设置位移量

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

//        // 在场景视图中绘制自定义位移轴
//        Handles.ArrowHandleCap(
//            GUIUtility.GetControlID(FocusType.Passive),
//            Vector3.zero,
//            Quaternion.LookRotation(customMoveAxis),
//            HandleUtility.GetHandleSize(Vector3.zero),
//            EventType.Repaint
//        );

//        // 处理用户输入并执行位移操作
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
