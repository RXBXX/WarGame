using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUD : UIBase
    {
        protected GameObject _gameObject;
        private Vector2 _offset;
        //public HUD(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        //{
        //    UILayer = Enum.UILayer.HUDLayer;
        //}

        public HUD(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            UILayer = Enum.UILayer.HUDLayer;
        }

        public void SetOwner(GameObject go)
        {
            this._gameObject = go;

            UpdatePosition();
        }

        public override void Update(float deltaTime)
        {
            UpdatePosition();
        }
        
        protected virtual void UpdatePosition()
        {
            var pos = CameraMgr.Instance.MainCamera.WorldToScreenPoint(_gameObject.transform.position);
            pos.y = Screen.height - pos.y;
            pos = GRoot.inst.GlobalToLocal(pos) + _offset;// new Vector2(pos.x / Screen.width * GRoot.inst.width, (Screen.height - pos.y) / Screen.height * GRoot.inst.height) + _offset;
            if (_gCom.position != pos)
                _gCom.position = pos;

            var dis = Vector3.Distance(CameraMgr.Instance.MainCamera.transform.position, _gameObject.transform.position);
            var scale = 10.0F / dis * Vector2.one;
            if (scale != _gCom.scale)
                _gCom.scale = scale;
        }

        public void UpdateOffset(Vector2 offset)
        {
            _offset = offset;
        }

        public override void Dispose(bool disposeGComp = false)
        {
            base.Dispose(disposeGComp);
            _gameObject = null;
        }
    }
}
