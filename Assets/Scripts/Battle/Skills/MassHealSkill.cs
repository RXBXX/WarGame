using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class MassHealSkill : MassSkill
    {
        public MassHealSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Cured_End, OnCuredEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Cured_End, OnCuredEnd);
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Cure" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoMassHeal(_initiatorID, _targets);
            }
        }

        protected override void TriggerSkill()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.Cure();
        }

        private void OnCuredEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (!_targets.Contains(targetID))
                return;

            _targets.Remove(targetID);
            if (_targets.Count > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }
    }
}