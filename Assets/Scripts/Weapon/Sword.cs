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

        protected override void EffectTake()
        {
            if (null != _trail)
                _trail.enabled = true;
        }

        protected override void EffectEnd()
        {
            if (null != _trail)
                _trail.enabled = false;
        }
    }
}
