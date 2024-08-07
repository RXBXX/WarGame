using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class EnemyBattleAction : BattleAction
    {
        public EnemyBattleAction(int id, int levelID) : base(id, levelID)
        {
        }

        public override void Dispose(bool save = false)
        {
            if (_initiatorID > 0)
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                if (initiator.GetState() > Enum.RoleState.Locked && initiator.GetState() < Enum.RoleState.Over)
                {
                    initiator.SetState(Enum.RoleState.Locked, true);
                    switch (initiator.GetState())
                    {
                        case Enum.RoleState.Moving:
                        case Enum.RoleState.WaitingOrder:
                        case Enum.RoleState.WatingTarget:
                        case Enum.RoleState.Attacking:
                            initiator.UpdateHexagonID(_path[0]);
                            break;
                        case Enum.RoleState.ReturnMoving:
                            initiator.UpdateHexagonID(_path[_path.Count - 1]);
                            break;
                    }
                }
            }

            base.Dispose(save);
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

            //DebugManager.Instance.Log("SkillID:" + _skillID);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AIAction_Start, args);
        }

        public void OnMoveStart(object[] args)
        {
            //try
            //{
            var inititor = RoleManager.Instance.GetRole(_initiatorID);
            inititor.SetState(Enum.RoleState.Moving);
            //}
            //catch
            //{
            //    DebugManager.Instance.Log(_initiatorID);
            //}
        }

        protected override void OnMoveEnd(params object[] args)
        {
            //DebugManager.Instance.Log("OnMoveEnd:" + _targetID);
            if (_targetID > 0)
            {
                OnAIAttack();
            }
            else
            {
                OnActionOver(new object[] { 0 });
            }
        }

        private void OnAIAttack()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.WatingTarget);
            _skillAction = Factory.Instance.GetSkill(_skillID, _initiatorID, _levelID);
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