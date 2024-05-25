using UnityEngine;
using FairyGUI;

namespace WarGame
{
    public class InputManager : Singeton<InputManager>
    {
        private Ray _ray;
        private RaycastHit _hitInfo;
        private Collider _downCollider = null;

        // Update is called once per frame
        public override void Update(float deltaTime)
        {
            if (null == CameraMgr.Instance.MainCamera)
                return;

            if (null != Stage.inst.touchTarget)
            {
                SceneMgr.Instance.Touch(null);
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
                }
                return;
            }

            _ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
            var raycast = Physics.Raycast(_ray, out _hitInfo);
            if (raycast)
            {
                SceneMgr.Instance.Touch(_hitInfo.collider.gameObject);
            }
            else
            {
                SceneMgr.Instance.Touch(null);
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
