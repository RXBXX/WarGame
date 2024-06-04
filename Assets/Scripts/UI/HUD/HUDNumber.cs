using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class HUDNumber : HUD
    {
        private GTextField _title;
        private Transition _showT;
        private Sequence _seq;

        public HUDNumber(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _title = (GTextField)_gCom.GetChild("title");
            _showT = (Transition)_gCom.GetTransition("show");
        }

        public void Show(float delay, string str, PlayCompleteCallback callback)
        {
            RemoveSequence();
            _seq = DOTween.Sequence();
            _seq.AppendInterval(delay);
            _seq.AppendCallback(()=>{
                _title.text = str;
                _showT.Play(callback);
            });
        }

        private void RemoveSequence()
        {
            if (null == _seq)
                return;

            _seq.Kill();
            _seq = null;
        }

        public override void Dispose(bool disposeGComp = false)
        {
            RemoveSequence();
            base.Dispose(disposeGComp);
        }
    }
}
