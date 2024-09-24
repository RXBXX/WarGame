using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroEquipItem : UIBase
    {
        private int _UID;
        private CommonHero _ownerIcon;
        private GButton _icon;
        private int _selectedRoleUID;
        private int _owner;

        public HeroEquipItem(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GButton>("icon");
            _ownerIcon = GetChild<CommonHero>("owner");

            gCom.onClick.Add(OnClick);
        }

        public void UpdateItem(int UID, bool adept, int owner, int selectedRoleUID)
        {
            _UID = UID;
            _selectedRoleUID = selectedRoleUID;
            _owner = owner;

            var equipData = DatasMgr.Instance.GetEquipmentData(UID);
            var title = equipData.GetConfig().GetTranslation("Name");
            _icon.title = title;

            if (0 != owner)
            {
                _ownerIcon.SetVisible(true);
                _ownerIcon.UpdateHero(DatasMgr.Instance.GetRoleData(owner).configId);
            }
            else
            {
                _ownerIcon.SetVisible(false);
            }

            _icon.icon = DatasMgr.Instance.GetEquipmentData(UID).GetConfig().Icon;
        }

        private void OnClick(EventContext context)
        {
            var pos = context.inputEvent.position;
            pos = GRoot.inst.GlobalToLocal(pos);

            var equipData = DatasMgr.Instance.GetEquipmentData(_UID);

            List<AttrStruct> _attrsData = new List<AttrStruct>();
            foreach (var v in equipData.GetAttrs())
            {
                _attrsData.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).GetTranslation("Name"), v.value.ToString()));
            }

            var btnTitle = "";
            if (_selectedRoleUID == _owner)
            {
                btnTitle = ConfigMgr.Instance.GetTranslation("HeroPanel_Unwear");
            }
            else
            {
                btnTitle = ConfigMgr.Instance.GetTranslation("HeroPanel_Wear");
            }


            WGCallback callback = OnWearCallback;
            var args = new object[] {
                equipData.GetName(),
            equipData.GetConfig().GetTranslation("Desc"),
            _attrsData,
                pos,
                true,
                btnTitle,
                "",
                false,
                callback
            };

            EventDispatcher.Instance.PostEvent(Enum.Event.Hero_Show_Talent, args);
        }

        private void OnWearCallback()
        {
            if (_owner == _selectedRoleUID)
                DatasMgr.Instance.UnwearEquipC2S(_selectedRoleUID, _UID);
            else
                DatasMgr.Instance.WearEquipC2S(_selectedRoleUID, _UID);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            _ownerIcon.Dispose();
            base.Dispose(disposeGCom);
        }
    }
}
