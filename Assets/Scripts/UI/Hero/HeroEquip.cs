using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroEquip : UIBase
    {
        private GTextField _title;
        private GLoader _ownerIcon;
        private GLoader _icon;
        
        public HeroEquip(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _title = GetUIChild<GTextField>("title");
            _ownerIcon = GetUIChild<GLoader>("ownerIcon");
            _icon = GetUIChild<GLoader>("icon");
        }

        public void UpdateItem(int UID, bool adept, int owner)
        {
            var equipData = DatasMgr.Instance.GetEquipmentData(UID);
            var title = equipData.GetConfig().Name;
            if (adept)
                title += "×¨¾«";
            _title.text = title;

            if (0 != owner)
            {
                _ownerIcon.url = DatasMgr.Instance.GetRoleData(owner).GetConfig().Icon;
            }
            else
            {
                _ownerIcon.url = "";
            }

            _icon.url = DatasMgr.Instance.GetEquipmentData(UID).GetConfig().Icon;
        }
    }
}
