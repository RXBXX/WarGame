//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(GameObject))]
//public class DraggableObjectEditor : Editor
//{
//    private GameObject selectedObject;

//    private void OnEnable()
//    {
//        // ��ȡ��ǰѡ�е���Ϸ����
//        selectedObject = (GameObject)target;
//    }

//    private void OnSceneGUI()
//    {
//        // ��ȡ��ǰ��Ϸ�����Transform���
//        Transform transform = selectedObject.transform;

//        // ����һ��Matrix4x4�������ڴ洢�����λ����Ϣ
//        Matrix4x4 handlerMatrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one * 0.5f);

//        // ʹ��HandleFunction���л��ƺͽ���
//        Handles.matrix = handlerMatrix;

//        // ����һ������ק���ᣬ����ȡ�µ�λ��
//        Vector3 newPosition = Handles.FreeMoveHandle(transform.position, Quaternion.identity, 1f, Vector3.zero, Handles.ArrowHandleCap);

//        // ��������λ�÷����˱仯����������λ��
//        if (newPosition != transform.position)
//        {
//            Undo.RecordObject(selectedObject, "Move Object");
//            Debug.Log("1111");
//            transform.position = newPosition;
//        }
//    }
//}