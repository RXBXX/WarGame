using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HUDFire : HUD
    {
        private int _id;
        private float _firedTime;
        private float _duration;
        private bool _isFired = false;
        private GTextField _timeTxt;
        private Controller _firedC;

        public HUDFire(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _id = (int)args[0];
            _duration = (float)args[1];
            _timeTxt = GetGObjectChild<GTextField>("time");
            _firedC = GetController("isFired");

            GetGObjectChild<GLoader>("icon").onClick.Add(OnClick);
        }

        public override void Update(float deltaTime)
        {
            if (_isFired)
            {
                _timeTxt.text = TimeMgr.Instance.GetFormatLeftTime((long)(_firedTime + _duration - TimeMgr.Instance.GetGameTime()) * 1000);
            }

            base.Update(deltaTime);
        }

        protected override void UpdatePosition()
        {
            var cameraFor = CameraMgr.Instance.GetMainCamForward();
            //Vector3.Dot(cameraFor.normalized, ownerFor.normalized) ��ֵ���� - 1 �� 1 ֮�䣺���ڸ�������Ĳ���ȷ�ԣ�������һ�������ĵ�����ܻ���΢���������ϵķ�Χ[-1, 1]��
            //Mathf.Acos �������ڳ��������Χ������ֵ�᷵�� NaN��
            if (cameraFor != Vector3.zero)
            {
                var ownerFor = _gameObject.transform.forward;
                ownerFor.y = 0;
                ownerFor = ownerFor.normalized;
                var cameraForXZ = cameraFor - new Vector3(0, cameraFor.y, 0);
                cameraForXZ = cameraForXZ.normalized;
                if (ownerFor != Vector3.zero)
                {
                    var angle = Mathf.Acos(Vector3.Dot(cameraForXZ, ownerFor)) / 2 / Mathf.PI * 360;
                    if (Vector3.Cross(cameraForXZ, ownerFor).y > 0)
                        angle = 360 - angle;

                    if (!float.IsNaN(angle))
                        _gCom.rotationY = angle;
                }

                var xAngle = Mathf.Acos(Vector3.Dot(cameraFor, cameraForXZ)) / 2 / Mathf.PI * 360;
                if (!float.IsNaN(xAngle))
                    _gCom.rotationX = xAngle;
            }
        }

        private void OnClick()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Fire, new object[] { _id });
        }

        public void OutFire()
        {
            _isFired = false;
            _firedC.SetSelectedIndex(0);
        }

        public void Fire(float firedTime)
        {
            _isFired = true;
            _firedTime = firedTime;
            _firedC.SetSelectedIndex(1);
        }
    }
}
