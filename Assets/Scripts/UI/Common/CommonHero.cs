using FairyGUI;

namespace WarGame.UI
{
    public class CommonHero : UIBase
    {
        public CommonHero(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
        }

        public void UpdateHero(int configID)
        {
            var config = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", configID);
            GetGObjectChild<GLoader>("icon").url = config.Icon;
            GetGObjectChild<GLoader>("element").url = ConfigMgr.Instance.GetConfig<ElementConfig>("ElementConfig", (int)config.Element).Icon;
        }
    }
}
