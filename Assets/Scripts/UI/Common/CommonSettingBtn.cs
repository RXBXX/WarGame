using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class CommonSettingBtn : UIBase
    {
        public CommonSettingBtn(GComponent gCom, string name, object[] args) : base(gCom, name, args)
        {
            gCom.onClick.Add(OnClick);
        }

        private void OnClick()
        {
            UIManager.Instance.OpenPanel("Settings", "SettingsPanel");
        }
    }
}
