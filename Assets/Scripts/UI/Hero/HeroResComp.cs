using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HeroResComp : UIBase
    {
        private GList _resList;
        private List<int> _resData = new List<int> {(int)Enum.ItemType.TalentRes, (int)Enum.ItemType.LevelRes };

        public HeroResComp(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            _resList = GetGObjectChild<GList>("resList");
            _resList.itemRenderer = OnItemRenderer;
            EventDispatcher.Instance.AddListener(Enum.Event.HeroLevelUpS2C, OnHeroLevelUpS2C);

            UpdateComp();
        }

        private   void UpdateComp()
        {
            _resList.numItems = _resData.Count;
            _resList.ResizeToFit();
        }

        private void OnItemRenderer(int index, GObject item)
        {
            var resItem = (GButton)item;
            resItem.title = DatasMgr.Instance.GetItem(_resData[index]).ToString();
            resItem.icon = ConfigMgr.Instance.GetConfig<ItemConfig>("ItemConfig", _resData[index]).Icon;
        }

        private void OnHeroLevelUpS2C(params object[] args)
        {
            UpdateComp();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HeroLevelUpS2C, OnHeroLevelUpS2C);
        }
    }
}
