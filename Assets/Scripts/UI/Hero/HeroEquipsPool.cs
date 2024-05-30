using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroEquipsPool : UIBase
    {
        private struct EquipStruct
        {
            public int uid;
            public int ownerUID;
            public bool adept;
            public Enum.EquipType type;

            public EquipStruct(int uid, Enum.EquipType type, int ownerUID, bool adept)
            {
                this.uid = uid;
                this.type = type;
                this.ownerUID = ownerUID;
                this.adept = adept;
            }
        }

        private Dictionary<Enum.EquipType, bool> _adeptEquipType = new Dictionary<Enum.EquipType, bool>();
        private Dictionary<int, int> _wearedEquipDic = new Dictionary<int, int>();
        private int _selectedEquip;
        private GTextField _title;
        private GList _list;
        private List<EquipStruct> _forcedEquipsData = new List<EquipStruct>();
        private Dictionary<string, HeroEquipItem> _forcedEquips = new Dictionary<string, HeroEquipItem>();

        public HeroEquipsPool(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _title = GetUIChild<GTextField>("title");
            _list = GetUIChild<GList>("list");
            _list.itemRenderer = OnForcedEquipRender;
            _list.onClickItem.Add(OnClickForcedEquip);

            gCom.onTouchBegin.Add(OnTouchBegin);
        }

        public void Show(int roleUID, int selectedEquipID)
        {
            _selectedEquip = selectedEquipID;
            var equip = DatasMgr.Instance.GetEquipmentData(_selectedEquip);

            var roleData = DatasMgr.Instance.GetRoleData(roleUID);
            var jobConfig = roleData.GetJobConfig();
            foreach (var v in jobConfig.AdeptEquipTypes)
            {
                _adeptEquipType[v] = true;
            }

            foreach (var v in DatasMgr.Instance.GetAllRoles())
            {
                var rd = DatasMgr.Instance.GetRoleData(v);
                foreach (var v1 in rd.equipmentDic)
                {
                    _wearedEquipDic[v1.Value] = rd.UID;
                }
            }

            _forcedEquipsData.Clear();
            var _equipsData = DatasMgr.Instance.GetAllEquipments();
            int ownerUID = 0;
            bool adept = false;
            foreach (var v in _equipsData)
            {
                var tempEquip = DatasMgr.Instance.GetEquipmentData(v);
                if (equip.GetForcedCombination() == tempEquip.GetType())
                {
                    ownerUID = _wearedEquipDic.ContainsKey(tempEquip.UID) ? _wearedEquipDic[tempEquip.UID] : 0;
                    adept = _adeptEquipType.ContainsKey(tempEquip.GetType()) ? true : false;
                    _forcedEquipsData.Add(new EquipStruct(tempEquip.UID, tempEquip.GetType(), ownerUID, adept));
                }
            }

            _list.numItems = _forcedEquipsData.Count;
            _list.ResizeToFit();

            SetVisible(true);
        }

        private void OnForcedEquipRender(int index, GObject item)
        {
            if (!_forcedEquips.ContainsKey(item.id))
                _forcedEquips.Add(item.id, new HeroEquipItem((GComponent)item));
            _forcedEquips[item.id].UpdateItem(_forcedEquipsData[index].uid, _forcedEquipsData[index].adept, _forcedEquipsData[index].ownerUID);
        }

        private void OnClickForcedEquip(EventContext context)
        {
            var index = _list.GetChildIndex((GObject)context.data);
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Wear_Equip, new object[] { _selectedEquip, _forcedEquipsData[index].uid });
        }

        private void OnTouchBegin(EventContext context)
        {
            var touchTarget = Stage.inst.touchTarget;
            while (null != touchTarget && touchTarget != GCom.displayObject)
            {
                touchTarget = touchTarget.parent;
            }
            SetVisible(false);
        }
    }
}
