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
                var starAttr = AttributeMgr.Instance.GetAttributeStr(config.ID, role.GetStarAttribute(attrType));
                var talentAttr = AttributeMgr.Instance.GetAttributeStr(config.ID, role.GetTalentAttribute(attrType));
                var equipAttr = AttributeMgr.Instance.GetAttributeStr(config.ID, role.GetEquipAttribute(attrType));
                _attrsData.Add(new FourStrPair(config.Name, starAttr, talentAttr, equipAttr));
            });
            _attrList.numItems = _attrsData.Count;
            _attrList.ResizeToFit();
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