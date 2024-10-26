using FairyGUI;

namespace WarGame.UI
{
    public class LoadPanel : UIBase
    {
        private GProgressBar _progress;

        public LoadPanel(GComponent gCom, string customName, object[] args):base(gCom, customName, args)
        {
            _progress = GetGObjectChild<GProgressBar>("progress");
            EventDispatcher.Instance.AddListener(Enum.Event.Scene_Load_Progress, OnUpdateProgress);
        }

        private void OnUpdateProgress(params object[] args)
        {
            //DebugManager.Instance.Log((float)args[0] * 100);
            _progress.value = (float)args[0] * 100;
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Scene_Load_Progress, OnUpdateProgress);
        }
    }
}
