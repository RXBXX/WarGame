using UnityEngine;
using FairyGUI;

namespace WarGame
{
    public class InputManager : Singeton<InputManager>
    {
        private Ray _ray;
        private RaycastHit _hitInfo;
        private Collider _downCollider = null;
        private float _rightMouseTime = 0;
        private float _rightMouseInterval = 0.1f;

        // Update is called once per frame
        public override void Update(float deltaTime)
        {
            if (null == CameraMgr.Instance.MainCamera)
                return;

            if (null != Stage.inst.touchTarget)
            {
                SceneMgr.Instance.FocusIn(null);
                if (Input.GetMouseButtonUp(0))
                {
                    _ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(_ray, out _hitInfo))
                    {
                        if (_hitInfo.collider == _downCollider)
                        {
                            SceneMgr.Instance.Click(_hitInfo.collider.gameObject);
                        }
                    }
                    _downCollider = null;

                    SceneMgr.Instance.ClickEnd();
                }

                if (Input.GetMouseButtonUp(1))
                {
                    SceneMgr.Instance.RightClickEnd();
                }
                return;
            }

            _ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
            var raycast = Physics.Raycast(_ray, out _hitInfo);
            if (raycast)
            {
                SceneMgr.Instance.FocusIn(_hitInfo.collider.gameObject);
            }
            else
            {
                SceneMgr.Instance.FocusIn(null);
            }

            if (Input.GetMouseButtonDown(0))
            {
                _ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _hitInfo))
                {
                    _downCollider = _hitInfo.collider;
                    SceneMgr.Instance.ClickBegin(_downCollider.gameObject);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _hitInfo))
                {
                    if (_hitInfo.collider == _downCollider)
                    {
                        SceneMgr.Instance.Click(_hitInfo.collider.gameObject);
                    }
                }
                _downCollider = null;

                SceneMgr.Instance.ClickEnd();
            }

            _rightMouseTime += deltaTime;
            if (_rightMouseTime >= _rightMouseInterval)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    //DebugManager.Instance.Log("MouseButtonDown");
                    _ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(_ray, out _hitInfo))
                    {
                        SceneMgr.Instance.RightClickBegin(_hitInfo.collider.gameObject);
                    }
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    //MouseButtonUp触发时，有极大概率会重复出发MouseButtonDown和MouseButtonUp
                    //DebugManager.Instance.Log("MouseButtonUp");
                    SceneMgr.Instance.RightClickEnd();
                    _rightMouseTime = 0;
                }
            }
        }

        public bool GetMouseButtonDown(int id)
        {
            return Input.GetMouseButtonDown(id);
        }

        public bool GetMouseButton(int id)
        {
            return Input.GetMouseButton(id);
        }

        public bool GetMouseButtonUp(int id)
        {
            return Input.GetMouseButtonUp(id);
        }

        public float GetAxis(string name)
        {
            return Input.GetAxis(name);
        }

        public Vector2 GetMousePos()
        {
            return Input.mousePosition;
        }

        public override bool Dispose()
        {
            base.Dispose();
            _downCollider = null;
            return true;
        }
    }
}
