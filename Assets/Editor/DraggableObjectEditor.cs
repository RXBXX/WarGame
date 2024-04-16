//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(GameObject))]
//public class DraggableObjectEditor : Editor
//{
//    private GameObject selectedObject;

//    private void OnEnable()
//    {
//        // 获取当前选中的游戏对象
//        selectedObject = (GameObject)target;
//    }

//    private void OnSceneGUI()
//    {
//        // 获取当前游戏对象的Transform组件
//        Transform transform = selectedObject.transform;

//        // 创建一个Matrix4x4矩阵，用于存储物体的位置信息
//        Matrix4x4 handlerMatrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one * 0.5f);

//        // 使用HandleFunction进行绘制和交互
//        Handles.matrix = handlerMatrix;

//        // 绘制一个可拖拽的轴，并获取新的位置
//        Vector3 newPosition = Handles.FreeMoveHandle(transform.position, Quaternion.identity, 1f, Vector3.zero, Handles.ArrowHandleCap);

//        // 如果物体的位置发生了变化，更新它的位置
//        if (newPosition != transform.position)
//        {
//            Undo.RecordObject(selectedObject, "Move Object");
//            Debug.Log("1111");
//            transform.position = newPosition;
//        }
//    }
//}