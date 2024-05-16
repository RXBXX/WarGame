namespace WarGame
{
    public class EnemyBattleAction : BattleAction
    {
        public EnemyBattleAction():base()
        {
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_Start, OnStart);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_MoveStart, OnMoveStart);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_AI_Start, OnStart);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_AI_MoveStart, OnMoveStart);
            base.RemoveListeners();
        }

        private void OnStart(object[] args)
        {
            _initiatorID = (int)args[0];
            _targetID = (int)args[1];
            _skillID = (int)args[2];
        }

        public void OnMoveStart(object[] args)
        {
            var inititor = RoleManager.Instance.GetRole(_initiatorID);
            inititor.SetState(Enum.RoleState.Moving);
        }

        protected override void OnMoveEnd(params object[] args)
        {
            //DebugManager.Instance.Log("OnMoveEnd" + _targetID);
            if (_targetID > 0)
            {
                OnAIAttack();
            }
            else
            {
                StandByInitiator();
            }
        }

        private void OnAIAttack()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.WatingTarget);
            _skillAction = SkillFactory.Instance.GetSkill(_skillID, _initiatorID);
            _skillAction.Start();

            _skillAction.ClickHero(_targetID);
            //_skillAction.Play(_targetID);
        }
    }
}