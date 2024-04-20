using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame
{
    public class InputManager : Singeton<InputManager>
    {
        private Collider _downCollider = null;
        private float _moveSpeed = 0.5F;

        // Update is called once per frame
        public void Update()
        {
            if ( null != Stage.inst.touchTarget)
                return;
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    if (hitInfo.collider.tag == Enum.Tag.Hexagon.ToString())
                    {
                        _downCollider = hitInfo.collider;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    if (hitInfo.collider.tag == Enum.Tag.Hexagon.ToString() && hitInfo.collider == _downCollider)
                    {
                        var hexagonPos = MapTool.Instance.FromWorldPosToCellPos(hitInfo.collider.transform.position);
                        MapManager.Instance.SelectHexagon(hexagonPos);
                    }
                }
                _downCollider = null;
            }
        }

        public void LateUpdate()
        {
            if (null != Stage.inst.touchTarget)
                return;

            //镜头前后移动
            float scrollValue = Input.GetAxis("Mouse ScrollWheel") * _moveSpeed;
            Camera.main.transform.position += Camera.main.transform.forward * scrollValue;

            //镜头xz平面移动
            if (Input.GetMouseButton(0))
            {
                float xAxis = Input.GetAxis("Mouse X") * _moveSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _moveSpeed;
                Camera.main.transform.position += Camera.main.transform.right * xAxis;
                Camera.main.transform.position += Camera.main.transform.up * yAxis;
            }

            //镜头旋转
            if (Input.GetMouseButton(1))
            {
                float xAxis = Input.GetAxis("Mouse X") * _moveSpeed;
                float yAxis = Input.GetAxis("Mouse Y") * _moveSpeed;

                var linePos = Camera.main.transform.position;
                var lineDir = Camera.main.transform.forward;
                var planePoint = Vector3.zero; //不同高度，这个点后面会动态变化
                var planeNorVec = Vector3.up;
                float t = (planeNorVec.x * (planePoint.x - linePos.x) + planeNorVec.y * (planePoint.y - linePos.y) + planeNorVec.z * (planePoint.z - linePos.z)) /
                    (planeNorVec.x * lineDir.x + planeNorVec.y * lineDir.y + planeNorVec.z * lineDir.z);
                var point = new Vector3(linePos.x + t * lineDir.x, linePos.y + t * lineDir.y, linePos.z + t * lineDir.z);
                Camera.main.transform.RotateAround(point, Vector3.up, xAxis);
                Camera.main.transform.RotateAround(point, Camera.main.transform.right, yAxis);
            }
        }

        public override bool Dispose()
        {
            base.Dispose();
            _downCollider = null;
            return true;
        }
    }
}
