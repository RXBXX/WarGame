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
        private float _maxDistance = 25.0f;
        private float _minDistance = 8.0f;
        private Tweener _tweener;
        private int _targetID;
        private float _cameraDis;
        private bool _isLocking = false;
        private bool _isLockingTarget = false;

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

            _cameraDis = _maxDistance;

            return true;
        }

        public override void LateUpdate()
        {
            if (_isLocking)
                return;

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
                if (tempCameraDis < _maxDistance && tempCameraDis > _minDistance)
                {
                    _cameraDis = tempCameraDis;

                    var target = RoleManager.Instance.GetRole(_targetID);
                    MainCamera.transform.position = target.GetPosition() - MainCamera.transform.forward * _cameraDis;
                }
            }

            if (InputManager.Instance.GetMouseButton(1))
            {
                float xAxis = Input.GetAxis("Mouse X") * _rotateSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _rotateSpeed;

                var target = RoleManager.Instance.GetRole(_targetID);
                var targetPos = target.GetPosition();
                MainCamera.transform.RotateAround(targetPos, Vector3.up, xAxis);

                var side1 = (targetPos - MainCamera.transform.position).normalized;
                var side2 = side1 - new Vector3(0, side1.y, 0);
                var angle = Mathf.Acos(Vector3.Distance(side2, Vector3.zero) / Vector3.Distance(side1, Vector3.zero)) * 180 / Mathf.PI;
                if (angle + yAxis > 10 && angle + yAxis < 60)
                {
                    MainCamera.transform.RotateAround(targetPos, MainCamera.transform.right, yAxis);
                }
            }

            if (!_isLockingTarget)
            {
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
            _tweener.onComplete = (() => { KillTweener(); });
        }

        public Vector3 GetMainCamPosition()
        {
            return MainCamera.transform.position;
        }

        public Vector3 GetMainCamForward()
        {
            return MainCamera.transform.forward;
        }

        private void ClearTarget()
        {
            if (0 != _targetID)
            {
                var oldTarget = RoleManager.Instance.GetRole(_targetID);
                if (null != oldTarget)
                    oldTarget.SetFollowing(false);

                _targetID = 0;
            }
        }

        public void SetTarget(int targetID)
        {
            if (_isLockingTarget)
                return;

            if (targetID == _targetID)
                return;
             
            KillTweener();
            ClearTarget();

            if (0 == targetID)
                return;

            _targetID = targetID;
            var target = RoleManager.Instance.GetRole(_targetID);
            target.SetFollowing(true);
            _tweener = MainCamera.transform.DOMove(target.GetPosition() - _cameraDis * MainCamera.transform.forward, 0.2F);
            _tweener.onComplete = (() => { KillTweener(); });
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
            if ((int)args[0] != _targetID)
                return;
            ClearTarget();
            FindingTarget();
        }

        private void FindingTarget()
        {
            ClearTarget();

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

        public void Lock()
        {
            _isLocking = true;
        }

        public void Unlock()
        {
            DebugManager.Instance.Log(_isLocking);
            _isLocking = false;
        }

        public void LockTarget()
        {
            _isLockingTarget = true;
        }

        public void UnlockTarget()
        {
            _isLockingTarget = false;
        }

        public override bool Dispose()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Role_Dispose, OnRoleDispose);

            ClearTarget();

            base.Dispose();
            return true;
        }
    }
}
