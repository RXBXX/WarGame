namespace WarGame
{
    public class EnemyBattleAction : BattleAction
    {
        public EnemyBattleAction()
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_Attack, OnAIAttack);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_Start, OnAIStart);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_Move, OnAIMove);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_Over, OnAIOver);
        }

        public override void Dispose()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_AI_Attack, OnAIAttack);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_AI_Start, OnAIStart);
            base.Dispose();
        }

        private void OnAIAttack(object[] args)
        {
            Attack((int)args[0], (int)args[1], (Enum.SkillType)args[2]);
        }

        private void OnAIStart(object[] args)
        {
            _initiatorID = (int)args[0];
        }

        private void OnAIMove(object[] args)
        {
            var inititor = RoleManager.Instance.GetRole(_initiatorID);
            inititor.SetState(Enum.RoleState.Moving);
        }

        private void OnAIOver(object[] args)
        {
            var inititor = RoleManager.Instance.GetRole(_initiatorID);
            inititor.SetState(Enum.RoleState.Over);
        }
    }
}