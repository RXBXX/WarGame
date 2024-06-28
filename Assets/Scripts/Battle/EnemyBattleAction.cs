using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class EnemyBattleAction : BattleAction
    {
        public EnemyBattleAction(int id):base(id)
        {
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            EventDispatcher.Instance.AddListener(Enum.Event.Fight_AI_Start, OnStart);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_AI_MoveStart, OnMoveStart);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_AI_Over, OnActionOver);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_AI_Start, OnStart);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_AI_MoveStart, OnMoveStart);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_AI_Over, OnActionOver);
            base.RemoveListeners();
        }

        private void OnStart(object[] args)
        {
            _initiatorID = (int)args[0];
            _targetID = (int)args[1];
            _skillID = (int)args[2];

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AIAction_Start, args);
        }

        public void OnMoveStart(object[] args)
        {
            var inititor = RoleManager.Instance.GetRole(_initiatorID);
            inititor.SetState(Enum.RoleState.Moving);
        }

        protected override void OnMoveEnd(params object[] args)
        {
            if (_targetID > 0)
            {
                OnAIAttack();
            }
            else
            {
                OnActionOver(new object[] { 0});
            }
        }

        private void OnAIAttack()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.WatingTarget);
            _skillAction = Factory.Instance.GetSkill(_skillID, _initiatorID);
            _skillAction.Start();

            _skillAction.ClickHero(_targetID);
        }

        protected override void OnActionOver(params object[] args)
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AIAction_Over);
            base.OnActionOver(args);
        }
    }
}