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
        private int _frameDeltaValue;
        private GTextField _valueTxt;

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
            _frameDeltaValue = _deltaValue / 25;
        }

        public override void Update(float deltaTime)
        {
            if (0 == _deltaValue)
                return;

            _deltaValue -= _frameDeltaValue;
            if (_deltaValue < 0)
            {
                _frameDeltaValue += _deltaValue;
                _deltaValue = 0;
            }

            _value += _frameDeltaValue;
            _valueTxt.text = _value.ToString();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
        }
    }
}
