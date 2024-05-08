using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUDBuff : UIBase
    {
        private int _buffID;
        private int _leftDuration;
        private GTextField _title;
        private GTextField _desc;

        public HUDBuff(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _title = (GTextField)gCom.GetChild("title");
            _desc = (GTextField)gCom.GetChild("desc");
            //gCom.onFocusIn.Add(OnTouchBegin);
            //gCom.onFocusOut.Add(OnTouchEnd);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Stage.inst.touchTarget == _gCom.displayObject)
            {
                if (!_desc.visible)
                    _desc.visible = true;
            }
            else
            {
                if (_desc.visible)
                    _desc.visible = false;
            }
        }

        public void UpdateBuff(int id, int leftDuration)
        {
            var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", id);
            _title.text = buffConfig.Name;
            _desc.text = string.Format(buffConfig.Desc, leftDuration);
        }
    }
}
