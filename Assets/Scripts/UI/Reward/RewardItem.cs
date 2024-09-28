using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class RewardItem : UIBase
    {
        private Sequence _seq;
        private WGCallback _callback;

        public RewardItem(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
        }

        public void UpdateItem(string icon, string title, float delay, WGCallback callback)
        {
            GetGObjectChild<GLoader>("icon").url = icon;
            GetGObjectChild<GTextField>("title").text = "x" + title;
            _seq = DOTween.Sequence();
            _seq.AppendInterval(delay);
            _seq.AppendCallback(()=> {
                AudioMgr.Instance.PlaySound("Assets/Audios/Reward.wav");
                GetTransition("fadeIn").Play(()=> { callback(); });
            });
            _seq.onComplete = () =>
            {
                _seq = null;
            };
        }

        public override void Dispose(bool disposeGCom = false)
        {
            if (null != _seq)
            {
                _seq.Kill();
                _seq = null;
            }

            base.Dispose(disposeGCom);
        }
    }
}
