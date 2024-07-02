using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroLevelComp : UIBase
    {
        private int _roleUID;
        private GList _levelList;
        private Dictionary<string, HeroLevelItem> _levelItemDic = new Dictionary<string, HeroLevelItem>();
        private int _level;

        public HeroLevelComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _levelList = GetGObjectChild<GList>("levelList");
            _levelList.itemRenderer = OnEquipRender;
        }

        public void UpdateComp(int roleUID)
        {
            _roleUID = roleUID;
            var roleData = DatasMgr.Instance.GetRoleData(_roleUID);
            _level = roleData.GetLevel();
            _levelList.numItems = roleData.GetConfig().MaxLevel;

            _levelList.ResizeToFit();
        }

        private void OnEquipRender(int index, GObject item)
        {
            if (!_levelItemDic.ContainsKey(item.id))
                _levelItemDic.Add(item.id, new HeroLevelItem((GComponent)item));
            _levelItemDic[item.id].UpdateItem(_roleUID, index+1, _level+1);
        }

        public void LevelUp(int level)
        {
            var roleData = DatasMgr.Instance.GetRoleData(_roleUID);
            _level = roleData.GetLevel();
            _levelList.numItems = roleData.GetConfig().MaxLevel;

            _levelList.ResizeToFit();
        }
    }
}
