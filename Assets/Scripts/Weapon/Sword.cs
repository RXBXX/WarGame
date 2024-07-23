using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Sword : Equip
    {
        public Sword(EquipmentData data, Transform spineRoot) : base(data, spineRoot)
        { 
        
        }

        public override void Attack(List<Vector3> hitPoss)
        {
            base.Attack(hitPoss);
            AudioMgr.Instance.PlaySound("sword_take.mp3");
        }
    }
}
