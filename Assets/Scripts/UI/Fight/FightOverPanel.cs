using System.Collections.Generic;
using FairyGUI;
using System.Collections;

namespace WarGame.UI
{
    class FightOverPanel : UIBase
    {
        private GList _herosList;
        private GList _enemysList;
        private List<int> _heros = new List<int>();
        private List<int> _enemys = new List<int>();
        private Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>> _reportDic;
        private Dictionary<string, FightReportItem> _heroReportsDic = new Dictionary<string, FightReportItem>();
        private Dictionary<string, FightReportItem> _enemyReportsDic = new Dictionary<string, FightReportItem>();
        private int _blurID;

        public FightOverPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;
            _blurID = RenderMgr.Instance.SetBlurBG(GetGObjectChild<GLoader>("bg"));
            _gCom.GetController("type").SetSelectedIndex((bool)args[0] ? 0 : 1);

            _herosList = GetGObjectChild<GList>("heros");
            _herosList.itemRenderer = OnHeroRender;
            _enemysList = GetGObjectChild<GList>("enemys");
            _enemysList.itemRenderer = OnEnemyRender;

            _gCom.onClick.Add(OnClick);

            _reportDic = (Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>>)args[2];
            foreach (var v in _reportDic[Enum.RoleType.Hero])
            {
                _heros.Add(v.Key);
            }
            _herosList.numItems = _heros.Count;

            foreach (var v in _reportDic[Enum.RoleType.Enemy])
            {
                _enemys.Add(v.Key);
            }
            _enemysList.numItems = _enemys.Count;
        }

        private void OnHeroRender(int index, GObject item)
        {
            if (!_heroReportsDic.ContainsKey(item.id))
            {
                _heroReportsDic[item.id] = UIManager.Instance.CreateUI<FightReportItem>("FightReportItem", item);
            }
            var role = DatasMgr.Instance.GetRoleData(_heros[index]);
            _heroReportsDic[item.id].UpdateItem(role.GetConfig().Icon, _reportDic[Enum.RoleType.Hero][_heros[index]]);
        }

        private void OnEnemyRender(int index, GObject item)
        {
            if (!_enemyReportsDic.ContainsKey(item.id))
            {
                _enemyReportsDic[item.id] = UIManager.Instance.CreateUI<FightReportItem>("FightReportItem", item);
            }
            var enemyConfig = ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", _enemys[index]);
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", enemyConfig.RoleID);
            _enemyReportsDic[item.id].UpdateItem(roleConfig.Icon, _reportDic[Enum.RoleType.Enemy][_enemys[index]]);
        }

        private void OnClick()
        {
            UIManager.Instance.ClosePanel(name);
            SceneMgr.Instance.DestroyBattleFiled();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            if (0 != _blurID)
            {
                RenderMgr.Instance.ReleaseBlurBG(_blurID);
                _blurID = 0;
            }
            base.Dispose(disposeGCom);
        }
    }
}
