using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        //镜头前后移动
        float scrollValue = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
        Camera.main.transform.position += Camera.main.transform.forward * scrollValue;

        //镜头xz平面移动
        if (Input.GetMouseButton(0))
        {
            float xAxis = Input.GetAxis("Mouse X") * Time.deltaTime;
            float yAxis = Input.GetAxis("Mouse Y") * Time.deltaTime;
            Camera.main.transform.position += Camera.main.transform.right * xAxis;
            Camera.main.transform.position += Camera.main.transform.up * yAxis;
        }

        //镜头旋转
        if (Input.GetMouseButton(1))
        {
            float xAxis = Input.GetAxis("Mouse X");
            float yAxis = Input.GetAxis("Mouse Y");

            var linePos = Camera.main.transform.position;
            var lineDir = Camera.main.transform.forward;
            var planePoint = Vector3.zero; //不同高度，这个点后面会动态变化
            var planeNorVec = Vector3.up;
            float x, y, z, t;
            t = (planeNorVec.x * (planePoint.x - linePos.x) + planeNorVec.y * (planePoint.y - linePos.y) + planeNorVec.z * (planePoint.z - linePos.z)) /
                (planeNorVec.x * lineDir.x + planeNorVec.y * lineDir.y + planeNorVec.z * lineDir.z);
            var point = new Vector3(linePos.x + t * lineDir.x, linePos.y + t * lineDir.y, linePos.z + t * lineDir.z);
            Debug.Log(point);
            point = Vector3.zero;
            Debug.Log(Camera.main.transform.up);
            Camera.main.transform.RotateAround(point, Vector3.up, xAxis);
            Camera.main.transform.RotateAround(point, Camera.main.transform.right, yAxis);
        }
    }
}
