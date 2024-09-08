using System.Collections.Generic;
using FairyGUI;

namespace WarGame.UI
{
    public class CommonResComp : UIBase
    {
        private GList _resList;
        private List<TwoIntPair> _resData;
        private Dictionary<int, CommonResItem> _itemDic = new Dictionary<int, CommonResItem>();

        public CommonResComp(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            _resList = GetGObjectChild<GList>("resList");
            _resList.itemRenderer = OnItemRenderer;
        }

        public void InitComp(List<TwoIntPair> resData)
        {
            _resData = resData;
            _resList.numItems = _resData.Count;
            _resList.ResizeToFit();
        }

        private void OnItemRenderer(int index, GObject item)
        {
            if (!_itemDic.ContainsKey(_resData[index].id))
                _itemDic.Add(_resData[index].id, UIManager.Instance.CreateUI<CommonResItem>("CommonResItem", item));
            _itemDic[_resData[index].id].Init(_resData[index]);
        }

        public void UpdateComp(List<TwoIntPair> resData)
        {
            foreach (var v in resData)
            {
                if (!_itemDic.ContainsKey(v.id))
                    continue;
                _itemDic[v.id].UpdateItem(v.value);
            }
            _resList.ResizeToFit();
        }

        public override void Update(float deltaTime)
        {
            foreach (var v in _itemDic)
                v.Value.Update(deltaTime);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
        }
    }
}
