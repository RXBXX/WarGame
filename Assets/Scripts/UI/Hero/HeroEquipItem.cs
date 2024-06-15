using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroEquipItem : UIBase
    {
        private int _UID;
        private GTextField _title;
        private GLoader _ownerIcon;
        private GButton _icon;
        private GButton _wearBtn;
        private int _selectedRoleUID;
        private int _owner;

        public HeroEquipItem(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _title = GetGObjectChild<GTextField>("title");
            _ownerIcon = GetGObjectChild<GLoader>("ownerIcon");
            _icon = GetGObjectChild<GButton>("icon");
            _wearBtn = GetGObjectChild<GButton>("wearBtn");

            _icon.onClick.Add(OnClickIcon);
            _wearBtn.onClick.Add(OnClickWearBtn);
        }

        public void UpdateItem(int UID, bool adept, int owner, int selectedRoleUID)
        {
            _UID = UID;
            _selectedRoleUID = selectedRoleUID;
            _owner = owner;

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

            if (owner == selectedRoleUID)
            {
                _wearBtn.title = "Ð¶ÏÂ";
            }
            else
            {
                _wearBtn.title = "´©´÷";
            }

            _icon.icon = DatasMgr.Instance.GetEquipmentData(UID).GetConfig().Icon;
        }

        private void OnClickIcon(EventContext context)
        {
            var pos = context.inputEvent.position;
            pos = GRoot.inst.GlobalToLocal(pos);

            var equipData = DatasMgr.Instance.GetEquipmentData(_UID);
            var attrs = new List<AttrStruct>();
            foreach (var v in equipData.GetAttrs())
            {
                attrs.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).Name, v.value.ToString()));
            }
            EventDispatcher.Instance.PostEvent(Enum.Event.Hero_Show_Attrs, new object[] { attrs, pos });
        }

        private void OnClickWearBtn()
        {
            if (_owner == _selectedRoleUID)
                DatasMgr.Instance.UnwearEquipC2S(_selectedRoleUID, _UID);
            else
                DatasMgr.Instance.WearEquipC2S(_selectedRoleUID, _UID);
        }
    }
}
