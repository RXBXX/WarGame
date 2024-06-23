using FairyGUI;

namespace WarGame.UI
{
    public class HeroShowPanel : UIBase
    {
        private WGArgsCallback _callback;

        public HeroShowPanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            var bg = GetGObjectChild<GLoader>("bg");
            bg.url = "UI/Background/HeroBG";
            bg.onClick.Add(OnClickBG);


            var heroId = (int)args[0];
            var heroConfig = ConfigMgr.Instance.GetConfig<HeroConfig>("HeroConfig", heroId);
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", heroConfig.RoleID);

            GetGObjectChild<GLoader>("hero").url = roleConfig.FullLengthIcon;
            GetGObjectChild<GTextField>("name").text = roleConfig.GetTranslation("Name");

            _callback = (WGArgsCallback)args[1];
        }

        private void OnClickBG()
        {
            UIManager.Instance.ClosePanel(name);
            if (null != _callback)
                _callback();
        }
    }
}
