using FairyGUI;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class FightPanel : UIBase
    {
        private GTextField _round;
        private FightInforComp _vsComp;
        private FightHeroGroup _heroGroup;
        private FightEnemyQueue _enemyQueue;
        private GButton _skipBtn;
        private FightRoleInfo _roleInfo;
        private int _levelID = 0;
        private CommonResComp _resComp;
        private GButton _readyBtn;
        private GTextField _tips;
        private CommonTime _time;

        public FightPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            _round = (GTextField)_gCom.GetChild("round");
            _vsComp = GetChild<FightInforComp>("vsComp");
            _heroGroup = GetChild<FightHeroGroup>("heroGroup");
            _enemyQueue = GetChild<FightEnemyQueue>("enemyQueue");
            _roleInfo = GetChild<FightRoleInfo>("roleInfo");
            _resComp = GetChild<CommonResComp>("resComp");
            _time = GetChild<CommonTime>("time");

            EventDispatcher.Instance.AddListener(Enum.Event.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_RoundChange_Event, OnStartEnemyTurn);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Infor_Show, OnVSShow);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Infor_Hide, OnVSHide);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Show_HeroGroup, OnShowHeroGroup);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Hide_HeroGroup, OnHideHeroGroup);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Show_RoleInfo, OnShowRoleInfo);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Hide_RoleInfo, OnHideRoleInfo);
            EventDispatcher.Instance.AddListener(Enum.Event.Item_Update, OnItemUpdate);
            EventDispatcher.Instance.AddListener(Enum.Event.Item_Update, OnItemUpdate);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_ShowReady, OnFightReady);

            _gCom.GetChild("closeBtn").onClick.Add(() =>
            {
                SceneMgr.Instance.DestroyBattleFiled();
                DatasMgr.Instance.Save();
            });

            _skipBtn = GetGObjectChild<GButton>("skipBtn");
            _skipBtn.title = ConfigMgr.Instance.GetTranslation("FightPanel_Skip");
            _skipBtn.onClick.Add(() =>
            {
                //DebugManager.Instance.Log("SkipClick");
                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Skip_Rount);
            });

            _tips = GetGObjectChild<GTextField>("readyTips");
            _tips.text = ConfigMgr.Instance.GetTranslation("FightPanel_StartTips");
            _readyBtn = GetGObjectChild<GButton>("startBtn");
            _readyBtn.title = ConfigMgr.Instance.GetTranslation("FightPanel_Start");
            _readyBtn.onClick.Add(() =>
            {
                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Start);
                _readyBtn.visible = false;
                _skipBtn.visible = true;
                _tips.visible = false;
            });

            _levelID = (int)args[0];

            if ((bool)args[1])
            {
                _readyBtn.visible = false;
                _skipBtn.visible = true;
                _tips.visible = false;
            }

            GetGObjectChild<GTextField>("title").text = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).GetTranslation("Name");
            _round.text = args[2].ToString();

            UpdateRess(true);
        }

        public override void Update(float deltaTime)
        {
            _resComp.Update(deltaTime);
            _time.Update(deltaTime);
        }

        private void OnUpdateRound(object[] args)
        {
            _round.text = ((int)args[0]).ToString();
        }

        private void OnStartEnemyTurn(object[] args)
        {
            if ((Enum.FightTurn)args[0] == Enum.FightTurn.HeroTurn)
            {
                BattleRoundFunc func = () => {
                    ((BattleRoundFunc)args[1]).Invoke();
                    _skipBtn.visible = true;
                };
                GetChild<FightTips>("tips").ShowTips(Enum.RoundType.OurTurn, func);
            }
            else
            {
                _skipBtn.visible = false;
                GetChild<FightTips>("tips").ShowTips(Enum.RoundType.EnemyTurn, (BattleRoundFunc)args[1]);
            }
        }

        private void OnVSShow(params object[] args)
        {
            _vsComp.Show((int)args[0], (Vector3)args[1]);
        }

        private void OnVSHide(params object[] args)
        {
            _vsComp.Hide();
        }

        private void OnShowHeroGroup(params object[] args)
        {
            _heroGroup.Show((Vector2)args[0], (List<int>)args[1]);;
        }

        private void OnHideHeroGroup(params object[] args)
        {
            _heroGroup.Hide();
        }

        private void OnShowRoleInfo(params object[] args)
        {
            _roleInfo.Show((Vector2)args[0], (int)args[1], (int)args[2]);
        }

        private void OnHideRoleInfo(params object[] args)
        {
            _roleInfo.Hide();
        }

        private void OnItemUpdate(params object[] args)
        {
            DebugManager.Instance.Log("OnFightDrops");
            UpdateRess(false);
        }

        private void UpdateRess(bool init = true)
        {
            var itemsDic = DatasMgr.Instance.GetLevelData(_levelID).GetItems();
            var ress = new List<TwoIntPair>();
            var itemID = (int)Enum.ItemType.LevelRes;
            //DebugManager.Instance.Log(itemsDic.ContainsKey(itemID) ? itemsDic[itemID] : 0);
            ress.Add(new TwoIntPair(itemID, itemsDic.ContainsKey(itemID) ? itemsDic[itemID] : 0));
            itemID = (int)Enum.ItemType.TalentRes;
            //DebugManager.Instance.Log(itemsDic.ContainsKey(itemID) ? itemsDic[itemID] : 0);
            ress.Add(new TwoIntPair(itemID, itemsDic.ContainsKey(itemID) ? itemsDic[itemID] : 0));
            itemID = (int)Enum.ItemType.EquipRes;
            //DebugManager.Instance.Log(itemsDic.ContainsKey(itemID) ? itemsDic[itemID] : 0);
            ress.Add(new TwoIntPair(itemID, itemsDic.ContainsKey(itemID) ? itemsDic[itemID] : 0));
            if (init)
                _resComp.InitComp(ress);
            else
                _resComp.UpdateComp(ress);
        }

        private void OnFightReady(params object[] args)
        {
            _readyBtn.visible = true;
            _tips.visible = true;
        }

        public override void Dispose(bool disposeGCom = false)
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Show_RoleInfo, OnShowRoleInfo);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Hide_RoleInfo, OnHideRoleInfo);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_RoundChange_Event, OnStartEnemyTurn);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Infor_Show, OnVSShow);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Infor_Hide, OnVSHide);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Show_HeroGroup, OnShowHeroGroup);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Hide_HeroGroup, OnHideHeroGroup);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Item_Update, OnItemUpdate);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_ShowReady, OnFightReady);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Item_Update, OnItemUpdate);

            base.Dispose(disposeGCom);
        }
    }
}
