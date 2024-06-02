using FairyGUI;
using UnityEngine;

namespace WarGame.UI
{
    public class FightPanel : UIBase
    {
        private GTextField _round;
        private GProgressBar _initiatorHP;
        private GProgressBar _targetHP;
        private Transition _showHP;
        private FightInforComp _vsComp;
        private FightHeroGroup _heroGroup;
        private FightEnemyQueue _enemyQueue;
        private GButton _skipBtn;

        public FightPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            _initiatorHP = GetUIChild<GProgressBar>("initiatorHP");
            _targetHP = GetUIChild<GProgressBar>("targetHP");
            _showHP = GetTransition("showHP");
            _round = (GTextField)_gCom.GetChild("round");
            _round.text = "0";

            _vsComp = GetChild<FightInforComp>("vsComp");
            _heroGroup = GetChild<FightHeroGroup>("heroGroup");
            _enemyQueue = GetChild<FightEnemyQueue>("enemyQueue");

            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_RoundChange_Event, OnStartEnemyTurn);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Infor_Show, OnVSShow);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Infor_Hide, OnVSHide);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Show_HeroGroup, OnShowHeroGroup);

            _gCom.GetChild("closeBtn").onClick.Add(() =>
            {
                SceneMgr.Instance.DestroyBattleFiled();
            });

            _skipBtn = GetUIChild<GButton>("skipBtn");
            _skipBtn.onClick.Add(() =>
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Skip_Rount);
            });

            var startBtn = GetUIChild<GButton>("startBtn");
            startBtn.onClick.Add(() =>
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Start);
                startBtn.visible = false;
                _skipBtn.visible = true;
            });

            DebugManager.Instance.Log("isReady:" + (bool)args[0]);
            if ((bool)args[0])
            {
                startBtn.visible = false;
                _skipBtn.visible = true;
            }

            _gCom.GetChild("settingsBtn").onClick.Add(() =>
            {
                UIManager.Instance.OpenPanel("Settings", "SettingsPanel");
            });
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
                GetChild<FightTips>("tips").ShowTips("Hero Turn", (RoundFunc)args[1]);
            }
            else
            {
                _skipBtn.visible = false;
                GetChild<FightTips>("tips").ShowTips("EnemyTurn", (RoundFunc)args[1]);
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

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_RoundChange_Event, OnStartEnemyTurn);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Infor_Show, OnVSShow);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Infor_Hide, OnVSHide);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Show_HeroGroup, OnShowHeroGroup);
        }
    }
}
