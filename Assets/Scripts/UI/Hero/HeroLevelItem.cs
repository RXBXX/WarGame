using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroLevelItem : UIBase
    {
        private GTextField _name;
        private Controller _type;
        private GButton _levelUPBtn;
        private GTextField _resText;
        private int _roleUID;
        private int _level;

        public HeroLevelItem(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("name");
            _type = GetController("type");
            _levelUPBtn = GetGObjectChild<GButton>("levelUpBtn");
            _resText = GetGObjectChild<GTextField>("resText");

            _levelUPBtn.onClick.Add(OnClickLevelUp);
        }

        public void UpdateItem(int roleUID, int index, int level)
        {
            _roleUID = roleUID;
            _level = index;

            _name.text = "Lv." + index;
            if (index < level)
            {
                _type.SetSelectedIndex(0);
            }
            else if (index == level)
            {
                var cost = DatasMgr.Instance.GetRoleData(roleUID).GetStarConfig().Cost;
                var own = DatasMgr.Instance.GetItem((int)Enum.ItemType.LevelRes);
                if (own >= cost)
                    _type.SetSelectedIndex(1);
                else
                    _type.SetSelectedIndex(2);
            }
            else
            {
                _type.SetSelectedIndex(3);
            }
        }

        private void OnClickLevelUp()
        {
            DatasMgr.Instance.HeroLevelUpC2S(_roleUID, _level);
        }
    }
}
