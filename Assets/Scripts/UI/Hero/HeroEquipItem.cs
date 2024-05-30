using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroEquipItem : UIBase
    {
        private int UID;
        private GTextField _title;
        private GLoader _ownerIcon;
        private GButton _icon;
        
        public HeroEquipItem(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _title = GetUIChild<GTextField>("title");
            _ownerIcon = GetUIChild<GLoader>("ownerIcon");
            _icon = GetUIChild<GButton>("icon");

            _icon.onClick.Add(OnClickIcon);
        }

        public void UpdateItem(int UID, bool adept, int owner)
        {
            this.UID = UID;
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

            _icon.icon = DatasMgr.Instance.GetEquipmentData(UID).GetConfig().Icon;
        }

        private void OnClickIcon(EventContext context)
        {
            var pos = context.inputEvent.position;
            pos = GRoot.inst.GlobalToLocal(pos);

            var equipData = DatasMgr.Instance.GetEquipmentData(UID);
            var attrs = new List<AttrStruct>();
            foreach (var v in equipData.GetAttrs())
            {
                attrs.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).Name, v.value.ToString()));
            }
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Show_Attrs, new object[] {attrs, pos});
        }
    }
}
