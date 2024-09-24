using FairyGUI;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame.UI
{
    public class FightReportItem : UIBase
    {
        private GTextField _PA, _MA, _Cure, _PD, _MD;
        private CommonHero _hero;
        private Transition _inLeft;
        private Transition _inRight;
        private Sequence _seq;

        public FightReportItem(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _hero = GetChild<CommonHero>("hero");
            _PA = GetGObjectChild<GTextField>("PA");
            _MA = GetGObjectChild<GTextField>("MA");
            _Cure = GetGObjectChild<GTextField>("Cure");
            _PD = GetGObjectChild<GTextField>("PD");
            _MD = GetGObjectChild<GTextField>("MD");

            _inLeft = GetTransition("inLeft");
            _inRight = GetTransition("inRight");
        }

        public void UpdateItem(int configId, Dictionary<Enum.AttrType, float> attrsDic)
        {
            _hero.UpdateHero(configId);

            _PA.text = attrsDic.ContainsKey(Enum.AttrType.PhysicalAttack) ? Mathf.Floor(attrsDic[Enum.AttrType.PhysicalAttack]).ToString() : "0";
            _MA.text = attrsDic.ContainsKey(Enum.AttrType.MagicAttack) ? Mathf.Floor(attrsDic[Enum.AttrType.MagicAttack]).ToString() : "0";
            _Cure.text = attrsDic.ContainsKey(Enum.AttrType.Cure) ? Mathf.Floor(attrsDic[Enum.AttrType.Cure]).ToString() : "0";
            _PD.text = attrsDic.ContainsKey(Enum.AttrType.PhysicalDefense) ? Mathf.Floor(attrsDic[Enum.AttrType.PhysicalDefense]).ToString() : "0";
            _MD.text = attrsDic.ContainsKey(Enum.AttrType.MagicDefense) ? Mathf.Floor(attrsDic[Enum.AttrType.MagicDefense]).ToString() : "0";
        }

        public void PlayInLeft(float delay)
        {
            _seq = DOTween.Sequence();
            _seq.AppendInterval(delay);
            _seq.AppendCallback(() =>
            {
                _inLeft.Play();
            });
            _seq.onComplete = RemoveSeq;
        }

        public void PlayInRight(float delay)
        {
            //DebugManager.Instance.Log("PlayInRight");
            _seq = DOTween.Sequence();
            _seq.AppendInterval(delay);
            _seq.AppendCallback(() =>
            {
                //DebugManager.Instance.Log("PlayInRight Play");
                _inRight.Play();
            });
            _seq.onComplete = RemoveSeq;
        }

        private void RemoveSeq()
        {
            if (null != _seq)
            {
                _seq.Kill();
                _seq = null;
            }
        }

        public override void Dispose(bool disposeGCom = false)
        {
            _hero.Dispose();
            RemoveSeq();

            base.Dispose(disposeGCom);
        }
    }
}
