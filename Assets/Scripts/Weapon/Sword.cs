using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Sword : Equip
    {
        public Sword(EquipmentData data) : base(data)
        { 
        
        }

        public override void Attack(Vector3 targetPos)
        {
            if (null != _trail)
                _trail.enabled = true;
        }

        public override void AttackEnd()
        {
            DebugManager.Instance.Log("AttackEnd");
            if (null != _trail)
                _trail.enabled = false;
        }
    }
}
