using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroLevelItem : UIBase
    {
        private GTextField _name;
        private Controller _highLight;
        private GButton _levelUPBtn;
        private int _roleUID;
        private int _level;

        public HeroLevelItem(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("name");
            _highLight = GetController("highLight");
            _levelUPBtn = GetGObjectChild<GButton>("levelUpBtn");

            _levelUPBtn.onClick.Add(OnClickLevelUp);
        }

        public void UpdateItem(int roleUID, int index, int level)
        {
            _roleUID = roleUID;
            _level = index;

            _name.text = "Lv." + index;
            _highLight.SetSelectedIndex(index == level ? 1 : 0);
            _levelUPBtn.visible = index == level + 1;
        }

        private void OnClickLevelUp()
        {
            DebugManager.Instance.Log("OnClickLevelUp");
            DatasMgr.Instance.HeroLevelUpC2S(_roleUID, _level);
        }
    }
}
