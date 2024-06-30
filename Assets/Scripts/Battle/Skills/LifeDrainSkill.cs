using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class LifeDrainSkill : FierceAttackSkill
    {
        public LifeDrainSkill(int id, int initiatorID) : base(id, initiatorID)
        { }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Attack" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoLefeDrain(_initiatorID, _targets[0]);
            }
        }
    }
}
