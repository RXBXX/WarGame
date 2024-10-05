using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class RageReductionSkill : MassSkill
    {
        public RageReductionSkill(int id, int initiatorID, int levelID) : base(id, initiatorID, levelID)
        {
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Cure_End, OnCureEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Cure_End, OnCureEnd);
        }


        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Cure" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoRageReduction(_initiatorID, _targets, 0.5F);
            }
        }

        protected override void TriggerSkill()
        {
            base.TriggerSkill();
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.Cure();
        }

        private void OnCureEnd(object[] args)
        {
            var sender = (int)args[0];
            if (sender != _initiatorID)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over());
        }
    }
}
