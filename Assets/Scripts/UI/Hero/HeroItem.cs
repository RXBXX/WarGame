using FairyGUI;

namespace WarGame.UI
{
    public class HeroItem : UIBase
    {
        private CommonHero _icon;
        private GTextField _name;

        public HeroItem(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _icon = GetChild<CommonHero>("icon");
            _name = GetGObjectChild<GTextField>("name");
        }

        public void UpdateHero(int uid)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            _icon.UpdateHero(roleData.configId);
            _name.text = roleData.GetConfig().GetTranslation("Name") ;
        }

        public override void Dispose(bool disposeGCom = false)
        {
            _icon.Dispose();
            base.Dispose(disposeGCom);
        }
    }
}

