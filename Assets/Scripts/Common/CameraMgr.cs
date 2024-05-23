using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame
{
    public class CameraMgr : Singeton<CameraMgr>
    {
        private float _moveSpeed = 0.5F;
        private float _zoomSpeed = 5.0f;
        private float _rotateSpeed = 2.0f;
        private Tweener _tweener;
        private GameObject _target;
        private float _cameraDis;

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

            if (null == _target)
                return;

            if (null != _tweener)
                return;

            //var linePos = MainCamera.transform.position;
            //var lineDir = MainCamera.transform.forward;
            //var planePoint = Vector3.zero;
            //var planeNorVec = Vector3.up;
            //float t = (planeNorVec.x * (planePoint.x - linePos.x) + planeNorVec.y * (planePoint.y - linePos.y) + planeNorVec.z * (planePoint.z - linePos.z)) /
            //    (planeNorVec.x * lineDir.x + planeNorVec.y * lineDir.y + planeNorVec.z * lineDir.z);
            //var point = new Vector3(linePos.x + t * lineDir.x, linePos.y + t * lineDir.y, linePos.z + t * lineDir.z);

            float scrollValue = Input.GetAxis("Mouse ScrollWheel");
            if (0 != scrollValue)
            {
                _cameraDis -= scrollValue * _zoomSpeed;
                if (_cameraDis < 25 && _cameraDis > 8)
                {
                    MainCamera.transform.position = _target.transform.position - MainCamera.transform.forward * _cameraDis;
                }
            }

            if (InputManager.Instance.GetMouseButton(1))
            {
                float xAxis = Input.GetAxis("Mouse X") * _rotateSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _rotateSpeed;

                MainCamera.transform.RotateAround(_target.transform.position, Vector3.up, xAxis);
                MainCamera.transform.RotateAround(_target.transform.position, MainCamera.transform.right, yAxis);
            }

            if (InputManager.Instance.GetMouseButton(0))
            {
                float xAxis = Input.GetAxis("Mouse X") * _moveSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _moveSpeed;
                var pos = MainCamera.transform.position - MainCamera.transform.right * xAxis - MainCamera.transform.up * yAxis;
                MainCamera.transform.position = pos;
            }
            else if (InputManager.Instance.GetMouseButtonUp(0))
            {
                var roles = RoleManager.Instance.GetAllRoles();
                var minRole = roles[0];
                var viewCenter = MainCamera.transform.position + MainCamera.transform.forward * _cameraDis;
                for (int i = 1; i < roles.Count; i++)
                {
                    if (Vector3.Distance(roles[i].GetPosition(), viewCenter) < Vector3.Distance(minRole.GetPosition(), viewCenter))
                    {
                        minRole = roles[i];
                    }
                }

                SetTarget(minRole.GameObject);
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
        }

        /// <summary>
        /// ����ѡ�����ʱ��������Ŀ��������ж���ָ�����
        /// </summary>
        public void CloseGray()
        {
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

            _tweener.onComplete = (() =>
            {
                _tweener.Kill();
                _tweener = null;
            });
        }

        public Vector3 GetMainCamPosition()
        {
            return MainCamera.transform.position;
        }

        public Vector3 GetMainCamForward()
        {
            return MainCamera.transform.forward;
        }

        public void SetTarget(GameObject go)
        {
            if (null == _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }

            _target = go;

            if (0 == _cameraDis)
            {
                _cameraDis = 15;
            }

            _tweener = MainCamera.transform.DOMove(_target.transform.position - _cameraDis * MainCamera.transform.forward, 0.2F);
            _tweener.onComplete = (() =>
            {
                _tweener.Kill();
                _tweener = null;
            });
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
