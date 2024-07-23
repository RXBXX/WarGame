using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace WarGame.UI
{
    public class CommonResItem : UIBase
    {
        private GLoader _icon;
        private int _value;
        private int _deltaValue;
        private GTextField _valueTxt;
        private float _interval = 0.04F;

        public CommonResItem(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GLoader>("icon");
            _valueTxt = GetGObjectChild<GTextField>("value");
        }

        public void Init(TwoIntPair resData)
        {
            _value = resData.value;
            _valueTxt.text = _value.ToString();
            _icon.url = ConfigMgr.Instance.GetConfig<ItemConfig>("ItemConfig", resData.id).Icon;
        }

        public void UpdateItem(int value)
        {
            _deltaValue = value - _value;
        }

        public override void Update(float deltaTime)
        {
            if (0 == _deltaValue)
                return;
            _interval -= deltaTime;
            if (_interval > 0)
                return;
            _interval = 0.06F;

            var delta = _deltaValue / Mathf.Abs(_deltaValue);
            _deltaValue -= delta;
            _value += delta;
            _valueTxt.text = _value.ToString();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
        }
    }
}
