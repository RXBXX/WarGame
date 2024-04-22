using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUDNumber : HUD
    {
        private GTextField _title;
        private Transition _showT;

        public HUDNumber(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _title = (GTextField)_gCom.GetChild("title");
            _showT = (Transition)_gCom.GetTransition("show");
        }

        public void Show(float hurt, PlayCompleteCallback callback)
        {
            _title.text = hurt.ToString();
            _showT.Play(callback);
        }
    }
}
