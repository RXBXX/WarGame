using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class CriticalHitSkill : SingleSkill
    {
        public CriticalHitSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Dead_End, OnDeadEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Dead_End, OnDeadEnd);
        }

        protected override void TriggerSkill()
        {
            base.TriggerSkill();
            RoleManager.Instance.GetRole(_initiatorID).Attack(RoleManager.Instance.GetRole(_targets[0]).GetEffectPos());
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Attack" == stateName && "Take" == secondStateName)
            {
                var multiply = (int)(GetConfig().Params[0]) / 100.0f;
                BattleMgr.Instance.DoCriticalHit(_initiatorID, _targets[0], multiply);
            }
        }

        private void OnAttackedEnd(object[] args)
        {
            var sender = (int)args[0];
            if (sender != _targets[0])
                return;

            var target = RoleManager.Instance.GetRole(sender);
            if (target.IsDead())
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targets[0])
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F, true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }
    }
}
