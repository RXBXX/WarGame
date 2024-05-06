using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroEquipComp : UIBase
    {
        private struct EquipmentStruct
        {
            public int UID;
            public int configId;
            public string name;
            public string icon;

            public EquipmentStruct(int UID, int configId, string name, string icon)
            {
                this.UID = UID;
                this.configId = configId;
                this.name = name;
                this.icon = icon;
            }
        }

        private int _job;
        private int _selectedIndex = 0;
        private GList _equipmentList = null;
        private List<int> _showEquipments = new List<int>();
        private int _selectedEquip;
        private List<int> _showTypes = new List<int>();
        private GList _typeList = null;

        public HeroEquipComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _typeList = (GList)_gCom.GetChild("typeList");
            _typeList.itemRenderer = OnTypeRenderer;
            _typeList.onClickItem.Add(OnClickType);

            _equipmentList = (GList)_gCom.GetChild("equipmentList");
            _equipmentList.itemRenderer = OnEquipmentRender;
            _equipmentList.onClickItem.Add(OnClickEquip);

            _gCom.GetChild("wearBtn").onClick.Add(OnWearEquip);
        }

        public void UpdateComp(int job)
        {
            _job = job;

            var jobConfig = ConfigMgr.Instance.GetConfig<RoleJobConfig>("RoleJobConfig", _job);
            _showTypes.AddRange(jobConfig.Equipments);
            _typeList.numItems = _showTypes.Count;

            UpdateEquipments();
        }

        private void UpdateEquipments()
        {
            var jobConfig = ConfigMgr.Instance.GetConfig<RoleJobConfig>("RoleJobConfig", _job);
            var equipmentType = jobConfig.Equipments[_selectedIndex];

            _showEquipments.Clear();
            var equipments = DatasMgr.Instance.GetAllEquipments();
            //DebugManager.Instance.Log(equipments.Length);
            for (int i = 0; i < equipments.Length; i++)
            {
                var equipmentData = DatasMgr.Instance.GetEquipmentData(equipments[i]);
                var equipmentConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipmentData.configId);
                if (equipmentConfig.Type == equipmentType)
                {
                    _showEquipments.Add(equipments[i]);
                }
            }
            _equipmentList.numItems = _showEquipments.Count;
        }

        private void OnEquipmentRender(int index, GObject item)
        {
            var equipmentData = DatasMgr.Instance.GetEquipmentData(_showEquipments[index]);
            var equipmentConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipmentData.configId);
            ((GButton)item).title = equipmentConfig.Name;
            ((GButton)item).icon = equipmentConfig.Icon;
        }

        private void OnTypeRenderer(int index, GObject item)
        {
            ((GButton)item).title = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", _showTypes[index]).Name;
        }

        private void OnClickType(EventContext context)
        {
            var index = _typeList.GetChildIndex((GObject)context.data);
            _selectedIndex = index;
            UpdateEquipments();
        }

        private void OnClickEquip(EventContext context)
        {
            var index = _equipmentList.GetChildIndex((GObject)context.data);
            //DebugManager.Instance.Log(index);
            _selectedEquip = _showEquipments[index];
        }

        private void OnWearEquip()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Wear_Equip, new object[] { _selectedEquip });
        }
    }
}
