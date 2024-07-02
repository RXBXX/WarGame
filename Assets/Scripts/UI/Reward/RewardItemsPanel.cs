using FairyGUI;

namespace WarGame.UI
{
    public class RewardItemsPanel : UIBase
    {
        private int _blurID;
        private WGArgsCallback _callback;
        private GList _rewardList;
        private ItemPair[] _items;

        public RewardItemsPanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            GetGObjectChild<GGraph>("mask").onClick.Add(OnClickBG);
            _blurID = RenderMgr.Instance.SetBlurBG(GetGObjectChild<GLoader>("bg"));

            _rewardList = GetGObjectChild<GList>("rewards");
            _rewardList.itemRenderer = OnItemRenderer;

            var rewardID = (int)args[0];
            _items = ConfigMgr.Instance.GetConfig<RewardConfig>("RewardConfig", rewardID).Rewards;
            _rewardList.numItems = _items.Length;

            _callback = (WGArgsCallback)args[1];
        }

        private void OnItemRenderer(int index, GObject item)
        {
            var gb = (GButton)item;
            gb.icon = ConfigMgr.Instance.GetConfig<ItemConfig>("ItemConfig", _items[index].id).Icon;
            gb.title = _items[index].value.ToString();
        }

        private void OnClickBG()
        {
            UIManager.Instance.ClosePanel(name);
            if (null != _callback)
                _callback();
        }
    }
}
