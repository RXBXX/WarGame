using FairyGUI;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class GMPanel : UIBase
    {
        private GList _levelList;
        private GButton _completeBtn;
        private List<int> _levels = new List<int>();
        private int _selectedID;

        public GMPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            UILayer = Enum.UILayer.PopLayer;
            _levelList = GetGObjectChild<GList>("levelList");
            _completeBtn = GetGObjectChild<GButton>("complete");

            _levelList.SetVirtual();
            _levelList.itemRenderer = OnItemRenderer;
            _levelList.onClickItem.Add(OnClickItem);

            _completeBtn.onClick.Add(OnComplete);

            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            ConfigMgr.Instance.ForeachConfig<LevelConfig>("LevelConfig", (config) =>
            {
                _levels.Add(config.ID);
            });
            _levelList.numItems = _levels.Count;
            _selectedID = _levels[0];
            _levelList.selectedIndex = 0;
        }

        private void OnItemRenderer(int index, GObject item)
        {
            ((GButton)item).title = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levels[index]).GetTranslation("Name");
        }

        private void OnClickItem(EventContext ec)
        {
            var index = _levelList.ChildIndexToItemIndex(_levelList.GetChildIndex((GObject)ec.data));
            _selectedID = _levels[index];
        }

        private void OnComplete()
        {
            var levelConfig = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _selectedID);

            var itemsDic = new Dictionary<int, int>();
            var mapPlugin = Tool.Instance.ReadJson<LevelMapPlugin>(Application.streamingAssetsPath + "/" + levelConfig.Map);
            foreach (var v in mapPlugin.enemys)
            {
                var enemyConfig = ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", v.ID);
                var rewardConfig = ConfigMgr.Instance.GetConfig<RewardConfig>("RewardConfig", enemyConfig.Reward);
                foreach (var v1 in rewardConfig.Rewards)
                {
                    if (!itemsDic.ContainsKey(v1.id))
                        itemsDic.Add(v1.id, 0);
                    itemsDic[v1.id] += v1.value;
                }
            }

            var items = new List<TwoIntPair>();
            foreach (var v in itemsDic)
                items.Add(new TwoIntPair(v.Key, v.Value));
            DatasMgr.Instance.AddItems(items);
            WGCallback cb = () => { EventMgr.Instance.TriggerEvent(levelConfig.WinEvent); };
            UIManager.Instance.OpenPanel("Reward", "RewardItemsPanel", new object[] { items, cb });
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
        }
    }
}
