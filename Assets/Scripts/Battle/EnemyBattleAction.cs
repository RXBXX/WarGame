namespace WarGame
{
    public class EnemyBattleAction : BattleAction
    {
        public EnemyBattleAction()
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_Start, OnStart);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_MoveStart, OnMoveStart);
        }

        public override void Dispose()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_AI_Start, OnStart);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_AI_MoveStart, OnMoveStart);
            base.Dispose();
        }


        private void OnStart(object[] args)
        {
            _initiatorID = (int)args[0];
            _targetID = (int)args[1];
            _skillType = (Enum.SkillType)args[2];
        }

        public void OnMoveStart(object[] args)
        {
            var inititor = RoleManager.Instance.GetRole(_initiatorID);
            inititor.SetState(Enum.RoleState.Moving);
        }

        protected override void OnMoveEnd(params object[] args)
        {
            DebugManager.Instance.Log("OnMoveEnd" + _targetID);
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
            Attack();
        }
    }
}