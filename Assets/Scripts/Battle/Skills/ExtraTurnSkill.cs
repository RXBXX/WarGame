using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class ExtraTurnSkill : SingleSkill
    {
        public ExtraTurnSkill(int id, int initiatorID) : base(id, initiatorID)
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

        protected override void TriggerSkill()
        {
            RoleManager.Instance.GetRole(_initiatorID).Cure();
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Cure" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoExtraTurn(_initiatorID, _targets[0]);
            }
        }

        private void OnCuredEnd(object[] args)
        {
            var sender = (int)args[0];
            if (sender != _targets[0])
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }
    }
}
