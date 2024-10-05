using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class RewardItemsPanel : UIBase
    {
        private int _blurID;
        private WGCallback _callback;
        private GList _rewardList;
        private List<TwoIntPair> _itemsData = new List<TwoIntPair>();
        private Dictionary<string, RewardItem> _itemsDic = new Dictionary<string, RewardItem>();

        public RewardItemsPanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            _blurID = RenderMgr.Instance.SetBlurBG(GetGObjectChild<GLoader>("bg"));
            GetGObjectChild<GLabel>("title").title = ConfigMgr.Instance.GetTranslation("RewardItemsPanel_Title");
            _rewardList = GetGObjectChild<GList>("rewards");
            _rewardList.itemRenderer = OnItemRenderer;

            _itemsData = (List<TwoIntPair>)args[0];
            _rewardList.numItems = _itemsData.Count;

            _callback = (WGCallback)args[1];
        }

        private void OnItemRenderer(int index, GObject item)
        {
            if (!_itemsDic.ContainsKey(item.id))
                _itemsDic.Add(item.id, UIManager.Instance.CreateUI<RewardItem>("RewardItem", item));

            var icon = ConfigMgr.Instance.GetConfig<ItemConfig>("ItemConfig", _itemsData[index].id).Icon;
            var title = _itemsData[index].value.ToString();
            _itemsDic[item.id].UpdateItem(icon, title, index * 0.2F, () =>
            {
                if (index >= _itemsData.Count - 1)
                {
                    GetGObjectChild<GGraph>("mask").onClick.Add(OnClickBG);
                    GetGObjectChild<GComponent>("tips").visible = true;
                }
            });
        }

        private void OnClickBG()
        {
            UIManager.Instance.ClosePanel(name);
            if (null != _callback)
                _callback();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            foreach (var v in _itemsDic)
                v.Value.Dispose();
            _itemsDic.Clear();

            if (0 != _blurID)
            {
                RenderMgr.Instance.ReleaseBlurBG(_blurID);
                _blurID = 0;
            }

            base.Dispose(disposeGCom);
        }
    }
}
