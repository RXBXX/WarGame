using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class SettingsFightItem : UIBase
    {
        private Enum.FightType _type;
        private GTextField _titie;
        private GTextField _value;

        public SettingsFightItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _titie = GetGObjectChild<GTextField>("title");
            _value = GetGObjectChild<GTextField>("value");

            GetGObjectChild<GButton>("leftBtn").onClick.Add(OnClickLeft);
            GetGObjectChild<GButton>("rightBtn").onClick.Add(OnClickRight);
        }

        public void UpdateItem(Enum.FightType type)
        {
            _type = type;
            _titie.text = ConfigMgr.Instance.GetTranslation("SettingsPanel_BattlePerformance");

            _value.text = DatasMgr.Instance.GetSkipBattle() ? ConfigMgr.Instance.GetTranslation("SettingsPanel_Off") : ConfigMgr.Instance.GetTranslation("SettingsPanel_On");
        }

        private void OnClickLeft(EventContext context)
        {
            var skipBattle = !DatasMgr.Instance.GetSkipBattle();
            DatasMgr.Instance.SetSkipBattle(skipBattle);

            _value.text = DatasMgr.Instance.GetSkipBattle() ? ConfigMgr.Instance.GetTranslation("SettingsPanel_Off") : ConfigMgr.Instance.GetTranslation("SettingsPanel_On");
        }

        private void OnClickRight(EventContext context)
        {
            var skipBattle = !DatasMgr.Instance.GetSkipBattle();
            DatasMgr.Instance.SetSkipBattle(skipBattle);

            _value.text = DatasMgr.Instance.GetSkipBattle() ? ConfigMgr.Instance.GetTranslation("SettingsPanel_Off") : ConfigMgr.Instance.GetTranslation("SettingsPanel_On");
        }
    }
}
