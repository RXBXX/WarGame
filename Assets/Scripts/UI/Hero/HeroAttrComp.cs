using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroAttrComp : UIBase
    {
        private int _heroUID;
        private GList _attrList;
        private Dictionary<string, HeroAttrItem> _attrItemsDic = new Dictionary<string, HeroAttrItem>();
        private List<FourStrPair> _attrsData = new List<FourStrPair>();

        public HeroAttrComp(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            _attrList = GetGObjectChild<GList>("attrList");
            _attrList.itemRenderer = OnAttrRenderer;
        }

        public void UpdateComp(int heroUID)
        {
            _heroUID = heroUID;
            UpdateAttrList();
        }

        private void UpdateAttrList()
        {
            _attrsData.Clear();
            var role = DatasMgr.Instance.GetRoleData(_heroUID);
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                var attrType = (Enum.AttrType)config.ID;
                _attrsData.Add(new FourStrPair(config.Name, role.GetStarAttribute(attrType).ToString(), role.GetTalentAttribute(attrType).ToString(), role.GetEquipAttribute(attrType).ToString()));
            });
            _attrList.numItems = _attrsData.Count;
            _attrList.ResizeToFit();

            DebugManager.Instance.Log(_attrList.height);
            DebugManager.Instance.Log(GCom.height);
        }

        private void OnAttrRenderer(int index, GObject item)
        {
            if (!_attrItemsDic.ContainsKey(item.id))
            {
                _attrItemsDic[item.id] = new HeroAttrItem((GComponent)item);
            }
            _attrItemsDic[item.id].UpdateItem(_attrsData[index]);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            foreach (var v in _attrItemsDic)
                v.Value.Dispose();
            _attrItemsDic.Clear();
        }
    }
}
