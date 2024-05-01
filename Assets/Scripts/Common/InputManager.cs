using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame
{
    public class InputManager : Singeton<InputManager>
    {
        private Collider _downCollider = null;

        // Update is called once per frame
        public override void Update(float deltaTime)
        {
            if (null != Stage.inst.touchTarget)
                return;

            if (null == CameraMgr.Instance.MainCamera)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                var ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    _downCollider = hitInfo.collider;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    if (hitInfo.collider == _downCollider)
                    {
                        SceneMgr.Instance.Click(hitInfo.collider.gameObject);
                    }
                }
                _downCollider = null;
            }
            else
            {
                var ray = CameraMgr.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    SceneMgr.Instance.Touch(hitInfo.collider.gameObject);
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
