using FairyGUI;

namespace WarGame.UI
{
    public class RewardHeroPanel : UIBase
    {
        private WGArgsCallback _callback;

        public RewardHeroPanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            var heroId = (int)args[0];
            var heroConfig = ConfigMgr.Instance.GetConfig<HeroConfig>("HeroConfig", heroId);
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", heroConfig.RoleID);

            GetChild<CommonSkillItem>("skill").Update(roleConfig.SpecialSkill);
            GetGObjectChild<GButton>("hero").icon = roleConfig.FullLengthIcon;
            GetGObjectChild<GTextField>("name").text = roleConfig.GetTranslation("Name");
            GetGObjectChild<GTextField>("desc").text = roleConfig.GetTranslation("Desc");

            _callback = (WGArgsCallback)args[1];

            GetTransition("fadeIn").Play(() =>
            {
                GetGObjectChild<GGraph>("bg").onClick.Add(OnClickBG);
            });

            AudioMgr.Instance.PlaySound("Assets/Audios/Equip_Buy.wav");
        }

        private void OnClickBG()
        {
            UIManager.Instance.ClosePanel(name);
            if (null != _callback)
                _callback();
        }
    }
}
