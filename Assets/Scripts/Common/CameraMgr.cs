using UnityEngine;
using FairyGUI;
using DG.Tweening;
using System.Collections.Generic;

namespace WarGame
{
    public class CameraMgr : Singeton<CameraMgr>
    {
        private float _moveSpeed = 0.5F;
        private float _zoomSpeed = 5.0f;
        private float _rotateSpeed = 2.0f;
        private float _maxDistance = 20.0f;
        private float _minDistance = 8.0f;
        private Tweener _tweener;
        private int _targetID;
        private float _cameraDis;
        private int _lockCount = 0;
        private bool _dragging = false;
        private Sequence _floatSeq;

        private bool _isLocking
        {
            get { return _lockCount > 0; }
            set
            {
                _lockCount += (value ? 1 : -1);
                if (_lockCount < 0)
                    _lockCount = 0;
            }

        }

        private int _lockTargetCount = 0;
        private bool _isLockingTarget
        {
            get { return _lockTargetCount > 0; }
            set 
            { 
                _lockTargetCount += (value ? 1 : -1);
                if (_lockTargetCount < 0)
                    _lockTargetCount = 0;
            }
        }

        public Camera MainCamera
        {
            get {
                return Camera.main; 
            }
        }

        public Camera UICamera
        {
            get { return FairyGUI.StageCamera.main; }
        }

        public override bool Init()
        {
            base.Init();

            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Role_Dispose, OnRoleDispose);

            _cameraDis = _maxDistance;

            return true;
        }

        public override void LateUpdate()
        {
            if (_isLocking)
                return;
            //DebugManager.Instance.Log("111111");
            if (null != Stage.inst.touchTarget)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (_dragging)
                    {
                        FindingTarget();
                        _dragging = false;
                    }
                }
                return;
            }
            //DebugManager.Instance.Log("222222");

            if (null == MainCamera)
                return;
            //DebugManager.Instance.Log("333333");
            if ("Main Camera" != MainCamera.name)
                return;
            //DebugManager.Instance.Log("444444");
            if (0 == _targetID)
                return;
            //DebugManager.Instance.Log("555555");
            if (null != _tweener)
                return;

            if (null != _floatSeq)
                return;
            //DebugManager.Instance.Log("666666");

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
                    MainCamera.transform.position = target.GetFollowPos() - MainCamera.transform.forward * _cameraDis;
                }
            }

            if (InputManager.Instance.GetMouseButton(1))
            {
                float xAxis = Input.GetAxis("Mouse X") * _rotateSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _rotateSpeed;

                var target = RoleManager.Instance.GetRole(_targetID);
                var targetPos = target.GetFollowPos();
                if (0 != xAxis)
                {
                    MainCamera.transform.RotateAround(targetPos, Vector3.up, xAxis);
                }

                if (0 != yAxis)
                {
                    var side1 = (targetPos - MainCamera.transform.position).normalized;
                    var side2 = side1 - new Vector3(0, side1.y, 0);
                    var angle = Mathf.Acos(Vector3.Distance(side2, Vector3.zero) / Vector3.Distance(side1, Vector3.zero)) * 180 / Mathf.PI;

                    if (angle + yAxis > 4 && angle + yAxis < 60)
                    {
                        MainCamera.transform.RotateAround(targetPos, MainCamera.transform.right, yAxis);
                    }
                    else if (yAxis < 0 && angle + yAxis > 60)
                    {
                        MainCamera.transform.RotateAround(targetPos, MainCamera.transform.right, yAxis);
                    }
                    else if (yAxis > 0 && angle + yAxis < 4)
                    {
                        MainCamera.transform.RotateAround(targetPos, MainCamera.transform.right, yAxis);
                    }
                }
            }

            //if (InputManager.Instance.GetMouseButton(0))
            //    DebugManager.Instance.Log(_isLockingTarget);
            if (!_isLockingTarget)
            {
                if (InputManager.Instance.GetMouseButton(0))
                {
                    //_xAxis = Input.GetAxis("Mouse X");
                    //_yAxis = Input.GetAxis("Mouse Y");
                    float xAxis = Input.GetAxis("Mouse X") * _moveSpeed;
                    float yAxis = Input.GetAxis("Mouse Y") * _moveSpeed;
                    if (xAxis != 0 || yAxis != 0)
                    {
                        _dragging = true;
                        var pos = MainCamera.transform.position - MainCamera.transform.right * xAxis - MainCamera.transform.up * yAxis;
                        MainCamera.transform.position = pos;
                    }
                }
                else if (InputManager.Instance.GetMouseButtonUp(0))
                {
                    if (_dragging)
                    {
                        FindingTarget();
                        _dragging = false;
                    }
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
            RenderMgr.Instance.ClosePostProcessiong(Enum.PostProcessingType.Gray);
        }

        public void ShakePosition()
        {
            KillTweener();
            _tweener = MainCamera.transform.DOShakePosition(0.5f, Random.Range(0.4F, 1.0F));
            _tweener.onComplete = (() => { KillTweener(); });
        }

        public Vector3 GetMainCamPosition()
        {
            return MainCamera.transform.position;
        }

        public Vector3 GetMainCamRight()
        {
            return MainCamera.transform.right;
        }

        public Vector3 GetMainCamForward()
        {
            return MainCamera.transform.forward;
        }

        private void ClearTarget()
        {
            //DebugManager.Instance.Log("ClearTarget");
            if (0 != _targetID)
            {
                var oldTarget = RoleManager.Instance.GetRole(_targetID);
                if (null != oldTarget)
                    oldTarget.SetFollowing(false);

                _targetID = 0;
            }
        }

        public void SetTarget(int targetID, bool ani = true)
        {
            if (targetID == _targetID)
                return;

            KillTweener();
            ClearTarget();

            if (0 == targetID)
                return;

            _targetID = targetID;
            var target = RoleManager.Instance.GetRole(_targetID);
            target.SetFollowing(true);
            if (ani)
            {
                var camDis = _cameraDis * MainCamera.transform.forward;
                _tweener = MainCamera.transform.DOMove(target.GetFollowPos() - camDis, 0.4F).SetEase(Ease.InOutQuad);
                _tweener.onComplete = (() => { KillTweener(); });
            }
            else
            {
                _cameraDis = _maxDistance;
                var forward = (new Vector3(11.0F, 1.4F, 6.928F) - new Vector3(-4.946F, 12.79f, -8.28f)).normalized;
                MainCamera.transform.forward = forward;
                MainCamera.transform.position = target.GetFollowPos() - _cameraDis * forward;
            }
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
            //DebugManager.Instance.Log(args[0]);
            if ((int)args[0] != _targetID)
                return;
            ClearTarget();

            FindingTarget(true);
        }

        private void FindingTarget(bool isInit = false)
        {
            var targetID = _targetID;
            ClearTarget();

            var roles = RoleManager.Instance.GetAllRoles();
            if (roles.Count <= 0)
                return;

            int minRoleID = targetID;
            float minRoleToViewCenterDirDistance = 0;

            if (0 != targetID)
            {
                minRoleID = targetID;
                var minRole = RoleManager.Instance.GetRole(minRoleID);
                minRoleToViewCenterDirDistance = Tool.GetDistancePointToLine(MainCamera.transform.position, MainCamera.transform.forward, minRole.GetFollowPos());
            }

            for (int i = 0; i < roles.Count; i++)
            {
                if (!isInit)
                {
                    if (!roles[i].InScreen())
                        continue;
                }


                var roleToViewCenterDirDistance = Tool.GetDistancePointToLine(MainCamera.transform.position, MainCamera.transform.forward, roles[i].GetFollowPos());
                if (0 == minRoleID || roleToViewCenterDirDistance < minRoleToViewCenterDirDistance)
                {
                    minRoleID = roles[i].ID;
                    minRoleToViewCenterDirDistance = roleToViewCenterDirDistance;
                }
            }

            SetTarget(minRoleID);
        }

        public bool Lock()
        {
            //DebugManager.Instance.Log("Lock");
            _isLocking = true;
            return _isLocking;
        }

        public void Unlock()
        {
            _isLocking = false;
        }

        public bool LockTarget()
        {
            //DebugManager.Instance.Log("LockTarget");
            _isLockingTarget = true;
            return _isLockingTarget;
        }

        public void UnlockTarget()
        {
            //DebugManager.Instance.Log("UnlockTarget");
            _isLockingTarget = false;
        }

        public void OpenBattleArena()
        {
            if (null != _tweener)
            {
                _tweener.Complete();
                _tweener.Kill();
                _tweener = null;
            }

            MainCamera.transform.position = new Vector3(100, 100, 100);
            OpenGray();
        }

        public void CloseBattleArena()
        {
            if (0 == _targetID)
                return;
            var target = RoleManager.Instance.GetRole(_targetID);
            MainCamera.transform.position = target.GetPosition() - MainCamera.transform.forward * _cameraDis;
            CloseGray();
        }

        //private Vector3 GetViewCenter()
        //{ 

        //}

        public void FloatPoints(List<WGVector3> points, WGArgsCallback callback)
        {
            DebugManager.Instance.Log("FloatPoints");
            if (null == points)
            {
                callback();
                return;
            }

            var camDis = _cameraDis * MainCamera.transform.forward;
            _floatSeq = DOTween.Sequence();
            for (int i = 0; i < points.Count; i++)
            {
                _floatSeq.Append(MainCamera.transform.DOMove(points[i].ToVector3() - camDis, 1.0F).SetEase(Ease.InOutQuad));
                _floatSeq.AppendInterval(0.4f);
            }
            var target = RoleManager.Instance.GetRole(_targetID);
            _floatSeq.Append(MainCamera.transform.DOMove(target.GetPosition() - camDis, 1.0F).SetEase(Ease.InOutQuad));
            _floatSeq.AppendCallback(() =>
            {
                callback();
                _floatSeq = null;
            });
            _floatSeq.onComplete = StopFloatPoint;
        }

        public void StopFloatPoint()
        {
            if (null != _floatSeq)
            {
                _floatSeq.Kill();
                _floatSeq = null;
            }
        }

        public override bool Dispose()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Role_Dispose, OnRoleDispose);

            if (null != _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }

            StopFloatPoint();

            ClearTarget();

            base.Dispose();
            return true;
        }
    }
}
