using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class TipsItem : UIBase
    {
        private Transition _show;
        private GTextField _desc;
        private Sequence _seq;

        public TipsItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.AlertLayer;
            _desc = GetGObjectChild<GTextField>("desc");
            _show = GetTransition("show");
        }

        public void Show(float delay, string str, WGArgsCallback callback)
        {
            SetVisible(false);
            //DebugManager.Instance.Log("TipsShow:true");
            _desc.text = str;
            _seq = DOTween.Sequence();
            _seq.AppendInterval(delay);
            _seq.AppendCallback(() =>
            {
                //DebugManager.Instance.Log("TipsVisible:true");
                SetVisible(true);
                _show.Play(() =>
                {
                    //DebugManager.Instance.Log("TipsVisible:false");
                    SetVisible(false);
                    callback(this);
                });
            });
            //_seq.onComplete = RemoveSeq;
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
            RemoveSeq();
            base.Dispose(disposeGCom);
        }
    }
}
