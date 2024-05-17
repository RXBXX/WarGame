using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class FightVSComp : UIBase
    {
        private Transition _vsC;

        public FightVSComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _vsC = GetTransition("vs");
        }

        public void Show()
        {
            _vsC.Play();
        }

        public void Hide()
        {
            _vsC.PlayReverse();
        }
    }
}
