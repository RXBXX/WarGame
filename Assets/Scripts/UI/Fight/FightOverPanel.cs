using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    class FightOverPanel : UIBase
    {
        public FightOverPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            _gCom.GetChild("result").text = (bool)args[0] ? "Win" : "Failed";
            _gCom.onClick.Add(()=> {
                UIManager.Instance.ClosePanel(name);
                SceneMgr.Instance.DestroyBattleFiled();
            });
        }
    }
}
