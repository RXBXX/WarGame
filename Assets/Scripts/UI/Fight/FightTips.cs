using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class FightTips : UIBase
    {
        private Transition _tipsT;
        private GTextField _title;
        public FightTips(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _title = (GTextField)_gCom.GetChild("title");
            _tipsT = _gCom.GetTransition("tips");
        }

        public void ShowTips(string title, BattleRoundFunc callback)
        {
            _gCom.visible = true;
            _title.text = title;
            _tipsT.Play(() => {
                _gCom.visible = false;
                callback.Invoke(); 
            });
        }
    }
}