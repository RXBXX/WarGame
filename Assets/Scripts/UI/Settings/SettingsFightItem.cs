using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class SettingsFightItem : UIBase
    {
        private Enum.FightType _type;
        private GTextField _titie;
        private GButton _checkBtn;

        public SettingsFightItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _titie = GetGObjectChild<GTextField>("title");
            _checkBtn = GetGObjectChild<GButton>("checkBtn");

            _checkBtn.onClick.Add(OnClick);
        }

        public void UpdateItem(Enum.FightType type)
        {
            _type = type;
            _titie.text = "’Ω∂∑±Ìœ÷:";
            _checkBtn.selected = DatasMgr.Instance.GetSkipBattle();
        }

        private void OnClick(EventContext context)
        {
            var skipBattle = !DatasMgr.Instance.GetSkipBattle();
            DatasMgr.Instance.SetSkipBattle(skipBattle);
            _checkBtn.selected = skipBattle;
        }
    }
}
