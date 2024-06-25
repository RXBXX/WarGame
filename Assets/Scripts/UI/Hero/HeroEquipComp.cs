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

        private GList _equipList = null;
        private List<EquipStruct> _equipsData = new List<EquipStruct>();
        private Dictionary<string, HeroEquipItem> _equipsDic = new Dictionary<string, HeroEquipItem>();
        private int _roleUID;

        public HeroEquipComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _equipList = GetGObjectChild<GList>("equipList");
            _equipList.itemRenderer = OnEquipRender;
        }

        public void UpdateComp(int roleUID)
        {
            _roleUID = roleUID;

            var roleData = DatasMgr.Instance.GetRoleData(_roleUID);
            var jobConfig = roleData.GetJobConfig();
            var adeptEquipType = new Dictionary<Enum.EquipType, bool>();
            foreach (var v in jobConfig.AdeptEquipTypes)
            {
                adeptEquipType[v] = true;
            }

            var wearedEquipDic = new Dictionary<int, int>();
            foreach (var v in DatasMgr.Instance.GetAllRoles())
            {
                var rd = DatasMgr.Instance.GetRoleData(v);
                foreach (var v1 in rd.equipmentDic)
                {
                    wearedEquipDic[v1.Value] = rd.UID;
                }
            }

            _equipsData.Clear();
            int ownerUID = 0;
            bool adept = false;
            Enum.EquipType type;
            EquipmentData equipData;
            foreach (var v in DatasMgr.Instance.GetAllEquipments())
            {
                ownerUID = wearedEquipDic.ContainsKey(v) ? wearedEquipDic[v] : 0;
                equipData = DatasMgr.Instance.GetEquipmentData(v);
                type = equipData.GetConfig().Type;
                adept = adeptEquipType.ContainsKey(type) ? true : false;
                _equipsData.Add(new EquipStruct(v, type, ownerUID, adept));
            }

            _equipList.numItems = _equipsData.Count;
            _equipList.ResizeToFit();
        }

        private void OnEquipRender(int index, GObject item)
        {
            if (!_equipsDic.ContainsKey(item.id))
                _equipsDic.Add(item.id, new HeroEquipItem((GComponent)item));
            _equipsDic[item.id].UpdateItem(_equipsData[index].uid, _equipsData[index].adept, _equipsData[index].ownerUID, _roleUID);
        }
    }
}
