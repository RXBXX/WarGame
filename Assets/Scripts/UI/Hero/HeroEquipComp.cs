using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroEquipComp : UIBase
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

        private int _selectedEquip;

        private GList _equipList = null;
        private List<EquipStruct> _equipsData = new List<EquipStruct>();
        private Dictionary<string, HeroEquipItem> _equipsDic = new Dictionary<string, HeroEquipItem>();

        //private GList _forcedEquipList;
        //private List<EquipStruct> _forcedEquipsData = new List<EquipStruct>();
        //private Dictionary<string, HeroEquip> _forcedEquips = new Dictionary<string, HeroEquip>();

        private GTextField _attrs;
        private GButton _wearBtn;

        //private HeroEquipsPool _forcedEquipComp;

        private int _roleUID;
        private Dictionary<Enum.EquipType, bool> _adeptEquipType = new Dictionary<Enum.EquipType, bool>();
        private Dictionary<int, int> _wearedEquipDic = new Dictionary<int, int>();

        public HeroEquipComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _equipList = GetGObjectChild<GList>("equipList");
            _equipList.itemRenderer = OnEquipRender;
            //_equipList.onClickItem.Add(OnClickEquip);

            //_forcedEquipComp = GetChild<HeroEquipsComp>("forcedEquip");

            ////_forcedEquipList = GetUIChild<GList>("forcedEquipList");
            ////_forcedEquipList.itemRenderer = OnForcedEquipRender;
            ////_forcedEquipList.onClickItem.Add(OnClickForcedEquip);

            //_wearBtn = GetUIChild<GButton>("wearBtn");
            //_wearBtn.onClick.Add(OnClickBtn);
            //_attrs = GetUIChild<GTextField>("attr");

            ////gCom.onTouchBegin.Add(OnTouchBegin);
        }

        public void UpdateComp(int roleUID)
        {
            _roleUID = roleUID;

            var roleData = DatasMgr.Instance.GetRoleData(_roleUID);
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

            _equipsData.Clear();
            int ownerUID = 0;
            bool adept = false;
            Enum.EquipType type;
            EquipmentData equipData;
            foreach (var v in DatasMgr.Instance.GetAllEquipments())
            {
                ownerUID = _wearedEquipDic.ContainsKey(v) ? _wearedEquipDic[v] : 0;
                equipData = DatasMgr.Instance.GetEquipmentData(v);
                type = equipData.GetConfig().Type;
                adept = _adeptEquipType.ContainsKey(type) ? true : false;
                _equipsData.Add(new EquipStruct(v, type, ownerUID, adept));
            }

            _selectedEquip = _equipsData[0].uid;
            _equipList.numItems = _equipsData.Count;
            _equipList.ResizeToFit();
        }

        private void OnEquipRender(int index, GObject item)
        {
            if (!_equipsDic.ContainsKey(item.id))
                _equipsDic.Add(item.id, new HeroEquipItem((GComponent)item));
            _equipsDic[item.id].UpdateItem(_equipsData[index].uid, _equipsData[index].adept, _equipsData[index].ownerUID, _roleUID);
        }


        //private void OnClickEquip(EventContext context)
        //{
        //    var index = _equipList.GetChildIndex((GObject)context.data);
        //    SelectEquip(_equipsData[index].uid);
        //}

        //private void OnForcedEquipRender(int index, GObject item)
        //{
        //    if (!_forcedEquips.ContainsKey(item.id))
        //        _forcedEquips.Add(item.id, new HeroEquip((GComponent)item));
        //    _forcedEquips[item.id].UpdateItem(_forcedEquipsData[index].uid, _forcedEquipsData[index].adept, _forcedEquipsData[index].ownerUID);
        //}

        //private void OnClickForcedEquip(EventContext context)
        //{
        //    var index = _forcedEquipList.GetChildIndex((GObject)context.data);
        //    EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Wear_Equip, new object[] { _selectedEquip, _forcedEquipsData[index].uid });
        //}


        //private void SelectEquip(int equipUID)
        //{
        //    _selectedEquip = equipUID;
        //    var attrStr = "";
        //    if (0 != _selectedEquip)
        //    {
        //        var equip = DatasMgr.Instance.GetEquipmentData(_selectedEquip);
        //        foreach (var v in equip.GetAttrs())
        //        {
        //            attrStr += ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).Name + ":+" + v.value + "\n";
        //        }
        //        _wearBtn.visible = true;

        //        var roleData = DatasMgr.Instance.GetRoleData(_roleUID);
        //        var place = equip.GetPlace();
        //        if (roleData.equipmentDic.ContainsKey(place) && roleData.equipmentDic[place] == _selectedEquip)
        //        {
        //            _wearBtn.title = "Unwear";
        //        }
        //        else
        //        {
        //            _wearBtn.title = "Wear";
        //        }
        //    }
        //    else
        //    {
        //        _wearBtn.visible = false;
        //    }
        //    _attrs.text = attrStr;
        //}

        //private void OnClickBtn()
        //{
        //    var roleData = DatasMgr.Instance.GetRoleData(_roleUID);
        //    var equip = DatasMgr.Instance.GetEquipmentData(_selectedEquip);
        //    var place = equip.GetPlace();
        //    if (roleData.equipmentDic.ContainsKey(place) && roleData.equipmentDic[place] == _selectedEquip)
        //    {
        //        UnwearEquip(equip);
        //    }
        //    else
        //    {
        //        WearEquip(equip);
        //    }
        //}

        //private void UnwearEquip(EquipmentData equip)
        //{
        //    EventDispatcher.Instance.PostEvent(Enum.Event.UnwearEquipS2C, new object[] { _selectedEquip });
        //}

        //private void WearEquip(EquipmentData equip)
        //{
        //if (0 == equip.GetForcedCombination())
        //{
        //    EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Wear_Equip, new object[] { _selectedEquip });
        //}
        //else
        //{
        //    _forcedEquipComp.Show(_roleUID, _selectedEquip);

        //_forcedEquipsData.Clear();
        //int ownerUID = 0;
        //bool adept = false;
        ////Enum.EquipType type;
        ////EquipmentData equipData;
        //foreach (var v in _equipsData)
        //{
        //    if (equip.GetForcedCombination() == v.type)
        //    {
        //        ownerUID = _wearedEquipDic.ContainsKey(v.uid) ? _wearedEquipDic[v.uid] : 0;
        //        adept = _adeptEquipType.ContainsKey(v.type) ? true : false;
        //        _forcedEquipsData.Add(new EquipStruct(v.uid, v.type, ownerUID, adept));
        //    }
        //}

        //_forcedEquipList.numItems = _forcedEquipsData.Count;
        //_forcedEquipList.visible = true;
        //    }
        //}

        //private void OnTouchBegin(EventContext context)
        //{
        //    var touchTarget = Stage.inst.touchTarget;
        //    while (null != touchTarget && touchTarget != _forcedEquipList.displayObject)
        //    {
        //        touchTarget = touchTarget.parent;
        //    }
        //    _forcedEquipList.visible = false;
        //}
    }
}
