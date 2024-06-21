using FairyGUI;

namespace WarGame.UI
{
    public class RewardPanel : UIBase
    {
        private WGArgsCallback _callback;

        public RewardPanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            GetGObjectChild<GGraph>("bg").onClick.Add(OnClickBG);

            var sp = (SourcePair)args[0];
            var txt = "";
            if (sp.Type == Enum.SourceType.Hero)
            {
                var heroConfig = ConfigMgr.Instance.GetConfig<HeroConfig>("HeroConfig", sp.id);
                txt = "¹§Ï²»ñµÃ" + ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", heroConfig.RoleID).Name;
            }
            GetGObjectChild<GTextField>("desc").text = txt;

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
