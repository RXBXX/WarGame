using FairyGUI;

namespace WarGame.UI
{
    class FightOverPanel : UIBase
    {
        private int _blurID;
        private FightOverComp _fightComp;

        public FightOverPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;
            _blurID = RenderMgr.Instance.SetBlurBG(GetGObjectChild<GLoader>("bg"));
            _gCom.GetController("type").SetSelectedIndex((bool)args[0] ? 0 : 1);

            GetGObjectChild<GGraph>("mask").onClick.Add(OnClick);

            _fightComp = GetChild<FightOverComp>("fightComp");
            _fightComp.UpdateComp(args);
        }

        private void OnClick()
        {
            UIManager.Instance.ClosePanel(name);
            SceneMgr.Instance.DestroyBattleFiled();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            if (0 != _blurID)
            {
                RenderMgr.Instance.ReleaseBlurBG(_blurID);
                _blurID = 0;
            }
            _fightComp.Dispose();
            base.Dispose(disposeGCom);
        }
    }
}
