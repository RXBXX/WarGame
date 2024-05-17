using FairyGUI;

namespace WarGame.UI
{
    public class FightPanel : UIBase
    {
        private GTextField _round;
        private GProgressBar _initiatorHP;
        private GProgressBar _targetHP;
        private Transition _showHP;
        private FightVSComp _vsComp;

        public FightPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            _gCom.GetChild("closeBtn").onClick.Add(()=> {
                SceneMgr.Instance.DestroyBattleFiled();
            });

            _initiatorHP = GetUIChild<GProgressBar>("initiatorHP");
            _targetHP = GetUIChild<GProgressBar>("targetHP");
            _showHP = GetTransition("showHP");
            _round = (GTextField)_gCom.GetChild("round");
            _round.text = "0";

            _vsComp = GetChild<FightVSComp>("vsComp");

            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_RoundChange_Event, OnStartEnemyTurn);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Show_HP, OnShowHP);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_HP_Change, OnHPChange);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Close_HP, OnCloseHP);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_VS_Show, OnVSShow);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_VS_Hide, OnVSHide);
        }

        private void OnUpdateRound(object[] args)
        {
            _round.text = ((int)args[0]).ToString();
            GetChild<FightTips>("tips").ShowTips("Hero Turn", (RoundFunc)args[1]);
        }

        private void OnStartEnemyTurn(object[] args)
        {
            GetChild<FightTips>("tips").ShowTips("EnemyTurn", (RoundFunc)args[0]);
        }

        private void OnShowHP(params object[] args)
        {
            var initiator = RoleManager.Instance.GetRole((int)args[0]);
            var target = RoleManager.Instance.GetRole((int)args[1]);
            _initiatorHP.GetController("style").SetSelectedIndex(initiator.Type == Enum.RoleType.Hero ? 0 : 1);
            _targetHP.GetController("style").SetSelectedIndex(target.Type == Enum.RoleType.Hero ? 0 : 1);
            _initiatorHP.value = initiator.GetHP();
            _targetHP.value = target.GetHP();
            _initiatorHP.visible = true;
            _targetHP.visible = true;
            _showHP.Play();
        }

        private void OnCloseHP(params object[] args)
        {
            _showHP.PlayReverse(()=>{
                _initiatorHP.visible = false;
                _targetHP.visible = false;
            });
        }

        private void OnHPChange(params object[] args)
        {
            var target = RoleManager.Instance.GetRole((int)args[0]);
            var hp = target.GetHP();
            _targetHP.TweenValue(hp, 0.5F);
        }

        private void OnVSShow(params object[] args)
        {
            _vsComp.Show();
        }

        private void OnVSHide(params object[] args)
        {
            _vsComp.Hide();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_RoundChange_Event, OnStartEnemyTurn);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Show_HP, OnShowHP);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_HP_Change, OnHPChange);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Close_HP, OnCloseHP);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_VS_Show, OnVSShow);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_VS_Hide, OnVSHide);
        }
    }
}
