using System.Collections.Generic;
using FairyGUI;
using System.Collections;

namespace WarGame.UI
{
    class FightOverComp : UIBase
    {
        private GLabel _title;
        private GList _herosList;
        private GList _enemysList;
        private List<int> _heros = new List<int>();
        private List<int> _enemys = new List<int>();
        private Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>> _reportDic;
        private Dictionary<string, FightReportItem> _heroReportsDic = new Dictionary<string, FightReportItem>();
        private Dictionary<string, FightReportItem> _enemyReportsDic = new Dictionary<string, FightReportItem>();
        private Transition _in;

        public FightOverComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _title = GetGObjectChild<GLabel>("title");
            _in = GetTransition("in");
        }

        public void UpdateComp(object[] args, WGCallback callback)
        {
            if ((bool)args[0])
            {
                _gCom.GetController("type").SetSelectedIndex(0);
                _title.title = ConfigMgr.Instance.GetTranslation("FightOverPanel_Win");
            }
            else
            {
                _gCom.GetController("type").SetSelectedIndex(1);
                _title.title = ConfigMgr.Instance.GetTranslation("FightOverPanel_Failed");
            }
            _herosList = GetGObjectChild<GList>("heros");
            _herosList.itemRenderer = OnHeroRender;
            _enemysList = GetGObjectChild<GList>("enemys");
            _enemysList.itemRenderer = OnEnemyRender;


            _in.SetHook("hook", () =>
            {
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
            });
            _in.Play(()=> { callback(); });

            GetGObjectChild<GTextField>("heroPhyAttack").text = GetGObjectChild<GTextField>("enemyPhyAttack").text = ConfigMgr.Instance.GetTranslation("FightOverPanel_PhyAttack");
            GetGObjectChild<GTextField>("heroMagAttack").text = GetGObjectChild<GTextField>("enemyMagAttack").text = ConfigMgr.Instance.GetTranslation("FightOverPanel_MagAttack");
            GetGObjectChild<GTextField>("heroCure").text = GetGObjectChild<GTextField>("enemyCure").text = ConfigMgr.Instance.GetTranslation("FightOverPanel_Cure");
            GetGObjectChild<GTextField>("heroPhyDefense").text = GetGObjectChild<GTextField>("enemyPhyDefense").text = ConfigMgr.Instance.GetTranslation("FightOverPanel_PhyDefense");
            GetGObjectChild<GTextField>("heroMagDefense").text = GetGObjectChild<GTextField>("enemyMagDefense").text = ConfigMgr.Instance.GetTranslation("FightOverPanel_MagDefense");
        }

        private void OnHeroRender(int index, GObject item)
        {
            if (!_heroReportsDic.ContainsKey(item.id))
            {
                _heroReportsDic[item.id] = UIManager.Instance.CreateUI<FightHeroReportItem>("FightHeroReportItem", item);
            }
            var role = DatasMgr.Instance.GetRoleData(_heros[index]);
            _heroReportsDic[item.id].UpdateItem(role.GetConfig().Icon, _reportDic[Enum.RoleType.Hero][_heros[index]]);
            _heroReportsDic[item.id].PlayInLeft(index * 0.1F);
        }

        private void OnEnemyRender(int index, GObject item)
        {
            if (!_enemyReportsDic.ContainsKey(item.id))
            {
                _enemyReportsDic[item.id] = UIManager.Instance.CreateUI<FightEnemyReportItem>("FightEnemyReportItem", item);
            }
            var enemyConfig = ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", _enemys[index]);
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", enemyConfig.RoleID);
            _enemyReportsDic[item.id].UpdateItem(roleConfig.Icon, _reportDic[Enum.RoleType.Enemy][_enemys[index]]);
            _enemyReportsDic[item.id].PlayInRight(index * 0.1F);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            foreach (var v in _heroReportsDic)
                v.Value.Dispose();
            _heroReportsDic.Clear();

            foreach (var v in _enemyReportsDic)
                v.Value.Dispose();
            _enemyReportsDic.Clear();

            base.Dispose(disposeGCom);
        }
    }
}
