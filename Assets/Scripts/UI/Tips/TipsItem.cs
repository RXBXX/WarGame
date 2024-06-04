using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class TipsItem : UIBase
    {
        private Transition _show;
        private GTextField _desc;

        public TipsItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.AlertLayer;
            _desc = GetGObjectChild<GTextField>("desc");
            _show = GetTransition("show");
        }

        public void Show(string str, WGArgsCallback callback)
        {
            _desc.text = str;
            _show.Play(()=> { callback(this); });
        }
    }
}
