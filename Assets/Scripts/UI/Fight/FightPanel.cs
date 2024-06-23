using FairyGUI;
using UnityEngine;

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

        public FightPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            _round = (GTextField)_gCom.GetChild("round");
            _vsComp = GetChild<FightInforComp>("vsComp");
            _heroGroup = GetChild<FightHeroGroup>("heroGroup");
            _enemyQueue = GetChild<FightEnemyQueue>("enemyQueue");
            _roleInfo = GetChild<FightRoleInfo>("roleInfo");

            EventDispatcher.Instance.AddListener(Enum.Event.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_RoundChange_Event, OnStartEnemyTurn);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Infor_Show, OnVSShow);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Infor_Hide, OnVSHide);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Show_HeroGroup, OnShowHeroGroup);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Hide_HeroGroup, OnHideHeroGroup);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Show_RoleInfo, OnShowRoleInfo);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Hide_RoleInfo, OnHideRoleInfo);

            _gCom.GetChild("closeBtn").onClick.Add(() =>
            {
                SceneMgr.Instance.DestroyBattleFiled();
            });

            _skipBtn = GetGObjectChild<GButton>("skipBtn");
            _skipBtn.onClick.Add(() =>
            {
                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Skip_Rount);
            });

            var startBtn = GetGObjectChild<GButton>("startBtn");
            startBtn.onClick.Add(() =>
            {
                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Start);
                startBtn.visible = false;
                _skipBtn.visible = true;
            });

            if ((bool)args[0])
            {
                startBtn.visible = false;
                _skipBtn.visible = true;
            }

            _round.text = args[1].ToString();
        }

        private void OnUpdateRound(object[] args)
        {
            _round.text = ((int)args[0]).ToString();
        }

        private void OnStartEnemyTurn(object[] args)
        {
            if ((Enum.FightTurn)args[0] == Enum.FightTurn.HeroTurn)
            {
                _skipBtn.visible = true;
                GetChild<FightTips>("tips").ShowTips("Hero", (BattleRoundFunc)args[1]);
            }
            else
            {
                _skipBtn.visible = false;
                GetChild<FightTips>("tips").ShowTips("Enemy", (BattleRoundFunc)args[1]);
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
            _heroGroup.Show((Vector2)args[0], (int[])args[1]);
        }

        private void OnHideHeroGroup(params object[] args)
        {
            _heroGroup.Hide();
        }

        private void OnShowRoleInfo(params object[] args)
        {
            _roleInfo.Show((Vector2)args[0], (int)args[1]);
        }

        private void OnHideRoleInfo(params object[] args)
        {
            _roleInfo.Hide();
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
            base.Dispose(disposeGCom);
        }
    }
}
