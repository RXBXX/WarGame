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

            _desc.text = str;
            _seq = DOTween.Sequence();
            _seq.AppendInterval(delay);
            _seq.AppendCallback(()=> {
                SetVisible(true);
                _show.Play(() => {
                    SetVisible(false);
                    callback(this); 
                });
            });
        }

        public override void Dispose(bool disposeGCom = false)
        {
            _seq.Kill();
            base.Dispose(disposeGCom);
        }
    }
}
