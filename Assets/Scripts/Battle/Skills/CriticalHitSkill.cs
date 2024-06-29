using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class CriticalHitSkill : FierceAttackSkill
    {
        public CriticalHitSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            //var initiator = RoleManager.Instance.GetRole(sender);
            if ("Attack" == stateName && "Take" == secondStateName)
            {
                var multiply = (int)(GetConfig().Params[0]) /100.0f;
                BattleMgr.Instance.DoCriticalHit(_initiatorID, _targetID, multiply);
            }
        }
    }
}
