using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class RouletteSkill : FierceAttackSkill
    {
        public RouletteSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            //var initiator = RoleManager.Instance.GetRole(sender);
            if ("Attack" == stateName && "Take" == secondStateName)
            {
                DebugManager.Instance.Log("HandleFightEvents");

                BattleMgr.Instance.DoRoulette(_initiatorID, _targetID);
            }
        }
    }
}
