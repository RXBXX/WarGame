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

        public void Show(float delay, string str, GTweenCallback callback)
        {
            RemoveSequence();
            _seq = DOTween.Sequence();
            _seq.AppendInterval(delay);
            _seq.AppendCallback(()=>{
                _title.text = str;
                var rd = Random.Range(- Mathf.PI / 3, Mathf.PI / 3);
                var offset = new Vector2(Mathf.Sin(rd), -Mathf.Cos(rd));
                var pos = _title.xy + offset * 80.0f;
                _title.TweenMove(pos, 0.3F);
                _title.scale = Vector2.zero;
                _title.TweenScale(Vector2.one, 0.3F);
            });
            _seq.AppendInterval(0.8F);
            _seq.AppendCallback(()=> {
                callback();
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
