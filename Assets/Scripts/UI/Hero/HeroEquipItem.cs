using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroEquipItem : UIBase
    {
        private int _UID;
        private GButton _ownerIcon;
        private GButton _icon;
        private int _selectedRoleUID;
        private int _owner;

        public HeroEquipItem(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GButton>("icon");
            _ownerIcon = GetGObjectChild<GButton>("owner");

            gCom.onClick.Add(OnClick);
        }

        public void UpdateItem(int UID, bool adept, int owner, int selectedRoleUID)
        {
            _UID = UID;
            _selectedRoleUID = selectedRoleUID;
            _owner = owner;

            var equipData = DatasMgr.Instance.GetEquipmentData(UID);
            var title = equipData.GetConfig().GetTranslation("Name");
            //if (adept)
            //    title += "×¨¾«";
            _icon.title = title;

            if (0 != owner)
            {
                _ownerIcon.visible = true;
                _ownerIcon.icon = DatasMgr.Instance.GetRoleData(owner).GetConfig().Icon;
            }
            else
            {
                _ownerIcon.visible = false;
            }

            //if (owner == selectedRoleUID)
            //{
            //    _wearBtn.title = "Ð¶ÏÂ";
            //}
            //else
            //{
            //    _wearBtn.title = "´©´÷";
            //}

            _icon.icon = DatasMgr.Instance.GetEquipmentData(UID).GetConfig().Icon;
        }

        private void OnClick(EventContext context)
        {
            var pos = context.inputEvent.position;
            pos = GRoot.inst.GlobalToLocal(pos);

            EventDispatcher.Instance.PostEvent(Enum.Event.Hero_Show_Attrs, new object[] { _UID, _owner, _selectedRoleUID, pos});
        }
    }
}
