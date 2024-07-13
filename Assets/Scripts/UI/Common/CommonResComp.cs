using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class CommonResComp : UIBase
    {
        private GList _resList;
        private List<int> _resData;

        public CommonResComp(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            _resList = GetGObjectChild<GList>("resList");
            _resList.itemRenderer = OnItemRenderer;
        }

        public void UpdateComp(List<int> resData)
        {
            _resData = resData;
            _resList.numItems = _resData.Count;
            _resList.ResizeToFit();
        }

        private void OnItemRenderer(int index, GObject item)
        {
            var resItem = (GButton)item;
            resItem.title = DatasMgr.Instance.GetItem(_resData[index]).ToString();
            resItem.icon = ConfigMgr.Instance.GetConfig<ItemConfig>("ItemConfig", _resData[index]).Icon;
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
        }
    }
}
