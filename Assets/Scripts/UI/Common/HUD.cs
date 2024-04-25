using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUD : UIBase
    {
        private GameObject _gameObject;
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

        public virtual void Update()
        {
            UpdatePosition();
        }
        
        private void UpdatePosition()
        {
            var pos = CameraMgr.Instance.MainCamera.WorldToScreenPoint(_gameObject.transform.position);
            
            pos = new Vector2(pos.x / Screen.width * GRoot.inst.width, (Screen.height - pos.y) / Screen.height * GRoot.inst.height);
            Debug.Log(GRoot.inst.width);
            if (_gCom.position != pos)
                _gCom.position = pos;

            var dis = Vector3.Distance(CameraMgr.Instance.MainCamera.transform.position, _gameObject.transform.position);
            var scale = 4.0f / dis * Vector2.one;
            if (scale != _gCom.scale)
                _gCom.scale = scale;
        }

        public override void Dispose(bool disposeGComp = false)
        {
            base.Dispose(disposeGComp);
            _gameObject = null;
        }
    }
}
