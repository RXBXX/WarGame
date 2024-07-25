using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class InspireSkill : MassSkill
    {
        public InspireSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Cured_End, OnCuredEnd);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Cure_End, OnCureEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Cured_End, OnCuredEnd);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Cure_End, OnCureEnd);
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Cure" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoInspire(_initiatorID, _targets);
            }
        }

        protected override void TriggerSkill()
        {
            base.TriggerSkill();
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

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over(1.5F));
        }

        private void OnCureEnd(object[] args)
        {
            var sender = (int)args[0];
            if (sender != _initiatorID)
                return;

            if (_targets.Count > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over(1.5F));
        }
    }
}
