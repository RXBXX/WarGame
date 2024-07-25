using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class DizzySkill : SingleSkill
    {
        public DizzySkill(int id, int initiatorID) : base(id, initiatorID)
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
            var hitPoss = new List<Vector3>();
            foreach (var v in _targets)
            {
                hitPoss.Add(RoleManager.Instance.GetRole(v).GetHitPos());
            }
            RoleManager.Instance.GetRole(_initiatorID).Attack(hitPoss);
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Attack" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoDizzy(_initiatorID, _targets[0]);
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

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over(1.5F));
        }

        private void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targets[0])
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over(1.5F, true));
        }
    }
}
