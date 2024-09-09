using FairyGUI;

namespace WarGame.UI
{
    class FightOverPanel : UIBase
    {
        private int _blurID;
        private FightOverComp _fightComp;
        private GGraph _mask;
        private GComponent _tips;
        private int _soundID;

        public FightOverPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;
            _blurID = RenderMgr.Instance.SetBlurBG(GetGObjectChild<GLoader>("bg"));
            _gCom.GetController("type").SetSelectedIndex((bool)args[0] ? 0 : 1);
            _mask = GetGObjectChild<GGraph>("mask");
            _tips = GetGObjectChild<GComponent>("tips");

            _mask.touchable = false;
            _tips.visible = false;

            _mask.onClick.Add(OnClick);

            _fightComp = GetChild<FightOverComp>("fightComp");
            _fightComp.UpdateComp(args, ()=> {
                _mask.touchable = true;
                _tips.visible = true;
            });

            if ((bool)args[0])
            {
                _soundID = AudioMgr.Instance.PlaySound("Assets/Audios/Win.wav");
            }
            else
            {
                _soundID = AudioMgr.Instance.PlaySound("Assets/Audios/Failed.wav");
            }
        }

        private void OnClick()
        {
            UIManager.Instance.ClosePanel(name);
            SceneMgr.Instance.DestroyBattleFiled();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            if (0 != _soundID)
            {
                AudioMgr.Instance.StopSound(_soundID);
                _soundID = 0;
            }

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
