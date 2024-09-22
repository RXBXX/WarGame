using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class SmithyEquipItem : UIBase
    {
        private GButton _icon;
        private GTextField _cost;
        private GLoader _res;
        private Transition _fadeIn;

        public SmithyEquipItem(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GButton>("icon");
            _cost = GetGObjectChild<GTextField>("cost");
            _res = GetGObjectChild<GLoader>("res");
            _res.url = ConfigMgr.Instance.GetConfig<ItemConfig>("ItemConfig", (int)Enum.ItemType.EquipRes).Icon;
            _fadeIn = GetTransition("fadeIn");
        }

        public void UpdateItem(int id)
        {
            var config = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", id);
            _icon.icon = config.Icon;
            _icon.title = config.GetTranslation("Name");
            _cost.text = config.Cost.ToString();
            _fadeIn.Play();
        }
    }
}
