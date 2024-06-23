using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUDBuff : UIBase
    {
        //private int _buffID;
        //private int _leftDuration;
        private GTextField _title;
        private GLoader _icon;

        public HUDBuff(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _title = (GTextField)gCom.GetChild("title");
            _icon = GetGObjectChild<GLoader>("icon");
        }

        //public override void Update(float deltaTime)
        //{
        //    base.Update(deltaTime);

        //    if (Stage.inst.touchTarget == _gCom.displayObject)
        //    {
        //        if (!_desc.visible)
        //            _desc.visible = true;
        //    }
        //    else
        //    {
        //        if (_desc.visible)
        //            _desc.visible = false;
        //    }
        //}

        public void UpdateBuff(int id, float leftDuration)
        {
            var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", id);
            _icon.url = buffConfig.Icon;
            _title.text = leftDuration.ToString();
            //_desc.text = string.Format(buffConfig.Desc, leftDuration);
        }
    }
}
