using FairyGUI;

namespace WarGame.UI
{
    public class HeroItem : UIBase
    {
        private GButton _icon;

        public HeroItem(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _icon = GetUIChild<GButton>("icon");
        }

        public void Update(string icon)
        {
            _icon.icon = icon;
        }
    }
}

