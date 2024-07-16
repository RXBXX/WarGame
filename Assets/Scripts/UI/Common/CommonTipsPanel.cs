using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class CommonTipsPanel : UIBase
    {
        private WGCallback _callback;

        public CommonTipsPanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            _callback = (WGCallback)args[1];
            GetGObjectChild<GTextField>("desc").text = (string)args[0];

            GetGObjectChild<GButton>("sureBtn").onClick.Add(OnSure);
            GetGObjectChild<GButton>("cancelBtn").onClick.Add(OnCancel);
        }

        private void OnSure()
        {
            UIManager.Instance.ClosePanel(name);
            _callback();
        }

        private void OnCancel()
        {
            UIManager.Instance.ClosePanel(name);
        }
    }
}
