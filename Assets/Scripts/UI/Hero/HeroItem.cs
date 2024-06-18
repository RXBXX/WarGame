using FairyGUI;

namespace WarGame.UI
{
    public class HeroItem : UIBase
    {
        private GButton _icon;
        private GTextField _name;

        public HeroItem(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GButton>("icon");
            _name = GetGObjectChild<GTextField>("name");
        }

        public void Update(string icon, string name)
        {
            _icon.icon = icon;
            _name.text = name;
        }
    }
}

