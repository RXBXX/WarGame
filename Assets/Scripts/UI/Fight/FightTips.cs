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
        private Controller _type;
        public FightTips(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _type = GetController("type");
            _title = (GTextField)_gCom.GetChild("title");
            _tipsT = _gCom.GetTransition("tips");
        }

        public void ShowTips(Enum.RoundType roundType, BattleRoundFunc callback)
        {
            _gCom.visible = true;
            _type.SetSelectedIndex((int)roundType);

            _tipsT.Play(() => {
                _tipsT.PlayReverse(() =>
                {
                    _gCom.visible = false;
                    callback.Invoke();
                });
            });
        }
    }
}