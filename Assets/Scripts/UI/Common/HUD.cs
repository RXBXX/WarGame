using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUD:UIBase
    {
        private GameObject _gameObject;
        public HUD(GComponent gCom, string name) : base(gCom, name)
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
            var pos = Camera.main.WorldToScreenPoint(_gameObject.transform.position);
            pos.y = GRoot._inst.height - pos.y;
            _gCom.position = pos;
        }

        public override void Dispose(bool disposeGComp = false)
        {
            base.Dispose(disposeGComp);
            _gameObject = null;
        }
    }
}
