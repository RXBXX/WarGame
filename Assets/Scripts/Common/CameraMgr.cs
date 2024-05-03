using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame
{
    public class CameraMgr : Singeton<CameraMgr>
    {
        private float _moveSpeed = 0.5F;
        private float _zoomSpeed = 3.0f;
        private float _rotateSpeed = 2.0f;

        public Camera MainCamera
        {
            get { return Camera.main; }
        }

        public Camera UICamera
        {
            get { return FairyGUI.StageCamera.main; }
        }

        public override bool Init()
        {
            base.Init();

            return true;
        }

        public override void LateUpdate()
        {
            if (null != Stage.inst.touchTarget)
                return;

            if (null == MainCamera)
                return;
            //求出摄像机射线和平面的交点
            var linePos = MainCamera.transform.position;
            var lineDir = MainCamera.transform.forward;
            var planePoint = Vector3.zero; //不同高度，这个点后面会动态变化
            var planeNorVec = Vector3.up;
            float t = (planeNorVec.x * (planePoint.x - linePos.x) + planeNorVec.y * (planePoint.y - linePos.y) + planeNorVec.z * (planePoint.z - linePos.z)) /
                (planeNorVec.x * lineDir.x + planeNorVec.y * lineDir.y + planeNorVec.z * lineDir.z);
            var point = new Vector3(linePos.x + t * lineDir.x, linePos.y + t * lineDir.y, linePos.z + t * lineDir.z);

            //镜头前后移动
            float scrollValue = Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed;
            MainCamera.transform.position += MainCamera.transform.forward * scrollValue;

            //镜头xz平面移动
            if (InputManager.Instance.GetMouseButton(0))
            {
                float xAxis = Input.GetAxis("Mouse X") * _moveSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _moveSpeed;
                var pos = MainCamera.transform.position - MainCamera.transform.right * xAxis - MainCamera.transform.up * yAxis;
                MainCamera.transform.position = pos;
            }

            //镜头旋转
            if (InputManager.Instance.GetMouseButton(1))
            {
                float xAxis = Input.GetAxis("Mouse X") * _rotateSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _rotateSpeed;

                MainCamera.transform.RotateAround(point, Vector3.up, xAxis);
                MainCamera.transform.RotateAround(point, MainCamera.transform.right, yAxis);
            }
        }

        public void SetMainCamera(Camera camera)
        {
            camera.tag = "MainCamera";
        }

        /// <summary>
        /// 攻击选择时，将攻击目标外的所有对象置灰
        /// </summary>
        public void OpenGray()
        {
            var grayCamera = MainCamera.transform.Find("GrayCamera");
            grayCamera.gameObject.SetActive(true);

            DebugManager.Instance.Log(Time.realtimeSinceStartup);
            MainCamera.GetComponent<CameraRender>().mat.SetFloat("_StartTime", Time.timeSinceLevelLoad);
            MainCamera.GetComponent<CameraRender>().enabled = true;
        }

        /// <summary>
        /// 攻击选择结束时，将攻击目标外的所有对象恢复正常
        /// </summary>
        public void CloseGray()
        {
            var grayCamera = MainCamera.transform.Find("GrayCamera");
            grayCamera.gameObject.SetActive(false);

            MainCamera.GetComponent<CameraRender>().enabled = false;
        }

        public override bool Dispose()
        {
            base.Dispose();

            return true;
        }
    }
}
