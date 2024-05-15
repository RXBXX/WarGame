using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame
{
    public class CameraMgr : Singeton<CameraMgr>
    {
        private float _moveSpeed = 0.5F;
        private float _zoomSpeed = 3.0f;
        private float _rotateSpeed = 2.0f;
        private Tweener _tweener;

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

            if ("Main Camera" != MainCamera.name)
                return;

            //�����������ߺ�ƽ��Ľ���
            var linePos = MainCamera.transform.position;
            var lineDir = MainCamera.transform.forward;
            var planePoint = Vector3.zero; //��ͬ�߶ȣ���������ᶯ̬�仯
            var planeNorVec = Vector3.up;
            float t = (planeNorVec.x * (planePoint.x - linePos.x) + planeNorVec.y * (planePoint.y - linePos.y) + planeNorVec.z * (planePoint.z - linePos.z)) /
                (planeNorVec.x * lineDir.x + planeNorVec.y * lineDir.y + planeNorVec.z * lineDir.z);
            var point = new Vector3(linePos.x + t * lineDir.x, linePos.y + t * lineDir.y, linePos.z + t * lineDir.z);

            //��ͷǰ���ƶ�
            float scrollValue = Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed;
            MainCamera.transform.position += MainCamera.transform.forward * scrollValue;

            //��ͷxzƽ���ƶ�
            if (InputManager.Instance.GetMouseButton(0))
            {
                float xAxis = Input.GetAxis("Mouse X") * _moveSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _moveSpeed;
                var pos = MainCamera.transform.position - MainCamera.transform.right * xAxis - MainCamera.transform.up * yAxis;
                MainCamera.transform.position = pos;
            }

            //��ͷ��ת
            if (InputManager.Instance.GetMouseButton(1))
            {
                float xAxis = Input.GetAxis("Mouse X") * _rotateSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _rotateSpeed;

                MainCamera.transform.RotateAround(point, Vector3.up, xAxis);
                MainCamera.transform.RotateAround(point, MainCamera.transform.right, yAxis);
            }
        }

        //�����������
        public void SetMainCamera(Camera camera)
        {
            camera.tag = "MainCamera";
        }

        /// <summary>
        /// ����ѡ��ʱ��������Ŀ��������ж����û�
        /// </summary>
        public void OpenGray()
        {
            RenderMgr.Instance.OpenPostProcessiong(Enum.PostProcessingType.Gray);
            //MainCamera.GetComponent<RenderMgr>().mat.SetFloat("_StartTime", Time.timeSinceLevelLoad);
            //MainCamera.GetComponent<RenderMgr>().OpenGrayEffect();
        }

        /// <summary>
        /// ����ѡ�����ʱ��������Ŀ��������ж���ָ�����
        /// </summary>
        public void CloseGray()
        {
            //var depthCamera = MainCamera.transform.Find("DepthCamera");
            //depthCamera.gameObject.SetActive(false);
            //var colorCamera = MainCamera.transform.Find("ColorCamera");
            //colorCamera.gameObject.SetActive(false);

            //MainCamera.GetComponent<RenderMgr>().CloseGrayEffect();
            RenderMgr.Instance.ClosePostProcessiong();
        }

        public void ShakePosition()
        {
            if (null == _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }
            _tweener = MainCamera.transform.DOShakePosition(0.5f, 0.1f);
        }

        public Vector3 GetMainCamPosition()
        {
            return MainCamera.transform.position;
        }

        public Vector3 GetMainCamForward()
        {
            return MainCamera.transform.forward;
        }

        public override bool Dispose()
        {
            base.Dispose();

            _tweener.Kill();
            _tweener = null;

            return true;
        }
    }
}
