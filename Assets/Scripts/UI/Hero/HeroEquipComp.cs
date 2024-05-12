using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroEquipComp : UIBase
    {
        private int _selectedEquip;

        private GList _equipList = null;
        private int[] _equipsData;

        private GList _forcedEquipList;
        private List<int> _forcedEquipData = new List<int>();

        private GTextField _attrs;
        private GButton _wearBtn;

        private int _roleUID;

        public HeroEquipComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _equipList = GetUIChild<GList>("equipList");
            _equipList.itemRenderer = OnEquipRender;
            _equipList.onClickItem.Add(OnClickEquip);

            _forcedEquipList = GetUIChild<GList>("forcedEquipList");
            _forcedEquipList.itemRenderer = OnForcedEquipRender;
            _forcedEquipList.onClickItem.Add(OnClickForcedEquip);

            _wearBtn = GetUIChild<GButton>("wearBtn");
            _wearBtn.onClick.Add(OnClickBtn);
            _attrs = GetUIChild<GTextField>("attr");

            gCom.onTouchBegin.Add(OnTouchBegin);
        }

        public void UpdateComp(int roleUID)
        {
            _roleUID = roleUID;

            _equipsData = DatasMgr.Instance.GetAllEquipments();
            _selectedEquip = _equipsData[0];

            _equipList.numItems = _equipsData.Length;
        }

        private void OnEquipRender(int index, GObject item)
        {
            var equip = DatasMgr.Instance.GetEquipmentData(_equipsData[index]);

            var btn = ((GButton)item);
            btn.title = equip.GetName();
            btn.icon = equip.GetIcon();
            btn.selected = _equipsData[index] == _selectedEquip;
        }


        private void OnClickEquip(EventContext context)
        {
            var index = _equipList.GetChildIndex((GObject)context.data);
            SelectEquip(_equipsData[index]);
        }

        private void OnForcedEquipRender(int index, GObject item)
        {
            var equip = DatasMgr.Instance.GetEquipmentData(_forcedEquipData[index]);

            var btn = ((GButton)item);
            btn.title = equip.GetName();
            btn.icon = equip.GetIcon();
            btn.selected = _equipsData[index] == _selectedEquip;
        }

        private void OnClickForcedEquip(EventContext context)
        {
            var index = _forcedEquipList.GetChildIndex((GObject)context.data);
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Wear_Equip, new object[] { _selectedEquip, _forcedEquipData[index]});
        }


        private void SelectEquip(int equipUID)
        {
            _selectedEquip = equipUID;
            var attrStr = "";
            if (0 != _selectedEquip)
            {
                var equip = DatasMgr.Instance.GetEquipmentData(_selectedEquip);
                foreach (var v in equip.GetAttrs())
                {
                    attrStr += ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).Name + ":+" + v.value + "\n";
                }
                _wearBtn.visible = true;

                var roleData = DatasMgr.Instance.GetRoleData(_roleUID);
                var place = equip.GetPlace();
                if (roleData.equipmentDic.ContainsKey(place) && roleData.equipmentDic[place] == _selectedEquip)
                {
                    _wearBtn.title = "Unwear";
                }
                else
                {
                    _wearBtn.title = "Wear";
                }
            }
            else
            {
                _wearBtn.visible = false;
            }
        }

        private void OnClickBtn()
        {
            var roleData = DatasMgr.Instance.GetRoleData(_roleUID);
            var equip = DatasMgr.Instance.GetEquipmentData(_selectedEquip);
            var place = equip.GetPlace();
            if (roleData.equipmentDic.ContainsKey(place) && roleData.equipmentDic[place] == _selectedEquip)
            {
                UnwearEquip(equip);
            }
            else
            {
                WearEquip(equip);
            }
        }

        private void UnwearEquip(EquipmentData equip)
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Unwear_Equip, new object[] { _selectedEquip });
        }

        private void WearEquip(EquipmentData equip)
        {
            if (0 == equip.GetForcedCombination())
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Wear_Equip, new object[] { _selectedEquip });
            }
            else
            {
                _forcedEquipData.Clear();
                for (int i = 0; i < _equipsData.Length; i++)
                {
                    var tempEquip = DatasMgr.Instance.GetEquipmentData(_equipsData[i]);
                    if (equip.GetForcedCombination() == tempEquip.GetType())
                        _forcedEquipData.Add(_equipsData[i]);

                }
                _forcedEquipList.numItems = _forcedEquipData.Count;
                _forcedEquipList.visible = true;
            }
        }

        private void OnTouchBegin(EventContext context)
        {
            var touchTarget = Stage.inst.touchTarget;
            while (null != touchTarget && touchTarget != _forcedEquipList.displayObject)
            {
                touchTarget = touchTarget.parent;
            }
            _forcedEquipList.visible = false;
        }
    }
}
