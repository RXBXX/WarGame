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
        private int _targetID;
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

            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Role_Dispose, OnRoleDispose);

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

            if (0 == _targetID)
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
                var tempCameraDis = _cameraDis - scrollValue * _zoomSpeed;
                if (tempCameraDis < 25 && tempCameraDis > 8)
                {
                    _cameraDis = tempCameraDis;

                    var target = RoleManager.Instance.GetRole(_targetID);
                    MainCamera.transform.position = target.GameObject.transform.position - MainCamera.transform.forward * _cameraDis;
                }
            }

            if (InputManager.Instance.GetMouseButton(1))
            {
                float xAxis = Input.GetAxis("Mouse X") * _rotateSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _rotateSpeed;

                var target = RoleManager.Instance.GetRole(_targetID);
                MainCamera.transform.RotateAround(target.GameObject.transform.position, Vector3.up, xAxis);
                MainCamera.transform.RotateAround(target.GameObject.transform.position, MainCamera.transform.right, yAxis);
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
                FindingTarget();
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
            KillTweener();
            _tweener = MainCamera.transform.DOShakePosition(0.5f, 0.1f);
            _tweener.onComplete = (() =>{KillTweener();});
        }

        public Vector3 GetMainCamPosition()
        {
            return MainCamera.transform.position;
        }

        public Vector3 GetMainCamForward()
        {
            return MainCamera.transform.forward;
        }

        public void SetTarget(int targetID)
        {
            KillTweener();
            if (0 != _targetID)
            {
                var oldTarget = RoleManager.Instance.GetRole(_targetID);
                oldTarget.SetFollowing(false);
            }

            _targetID = targetID;

            if (0 == _targetID)
            {
                _targetID = 0;
                _cameraDis = 0;
                return;
            }

            if (0 == _cameraDis)
            {
                _cameraDis = 15;
            }

            var target = RoleManager.Instance.GetRole(_targetID);
            target.SetFollowing(true);
            _tweener = MainCamera.transform.DOMove(target.GameObject.transform.position - _cameraDis * MainCamera.transform.forward, 0.2F);
            _tweener.onComplete = (() =>{KillTweener();});
        }

        private void KillTweener()
        {
            if (null != _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }
        }

        private void OnRoleDispose(params object[] args)
        {
            if (0 == _targetID)
                return;
            FindingTarget();
        }

        private void FindingTarget()
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

            SetTarget(minRole.ID);
        }

        public override bool Dispose()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Role_Dispose, OnRoleDispose);

            _tweener.Kill();
            _tweener = null;

            base.Dispose();
            return true;
        }
    }
}
