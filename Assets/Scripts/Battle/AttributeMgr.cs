using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class AttributeMgr : Singeton<AttributeMgr>
    {
        ///计算元素克制加成
        public float GetElementAdd(int _initiatorID, int targetID = 0)
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);

            var add = 0.0f;
            var initiatorElement = initiator.GetElement();

            if (0 != targetID)
            {
                var target = RoleManager.Instance.GetRole(targetID);
                if (target.GetElementConfig().Restrain == initiatorElement)
                {
                    add -= 0.1F;
                }
            }

            var hexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);
            foreach (var v in MapManager.Instance.Dicections)
            {
                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(MapTool.Instance.GetHexagonKey(hexagon.coor + v));
                if (0 == roleID || roleID == _initiatorID)
                    continue;

                var role = RoleManager.Instance.GetRole(roleID);
                if (initiator.Type == role.Type && role.GetElementConfig().Reinforce == initiatorElement)
                {
                    add += 0.1F;
                }
            }

            return add;
        }

        public float GetAttackPower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var add = GetElementAdd(initiatorID, targetID);
            var initiatorPhysicalAttack = initiator.GetAttribute(Enum.AttrType.PhysicalAttack) * (1 + add);
            var initiatorPhysicalAttackRatio = initiator.GetAttribute(Enum.AttrType.PhysicalAttackRatio) * (1 + add);
            var initiatorMagicAttack = initiator.GetAttribute(Enum.AttrType.MagicAttack) * (1 + add);
            var initiatorMagicAttackRatio = initiator.GetAttribute(Enum.AttrType.MagicAttackRatio) * (1 + add);
            var initiatorPhysicalPenetrateRatio = initiator.GetAttribute(Enum.AttrType.PhysicalPenetrateRatio) * (1 + add);
            var initiatorMagicPenetrateRatio = initiator.GetAttribute(Enum.AttrType.MagicPenetrateRatio) * (1 + add);

            var targetPhysicalDefense = target.GetAttribute(Enum.AttrType.PhysicalDefense);
            var targetMagicDefense = target.GetAttribute(Enum.AttrType.MagicDefense);

            var physicalHurt = initiatorPhysicalAttack * (1 + initiatorPhysicalAttackRatio) - (1 - initiatorPhysicalPenetrateRatio) * targetPhysicalDefense;
            var magicHurt = initiatorMagicAttack * (1 + initiatorMagicAttackRatio) - (1 - initiatorMagicPenetrateRatio) * targetMagicDefense;
            return physicalHurt + magicHurt;
        }

        public float GetCurePower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var add = GetElementAdd(initiatorID, targetID);
            return initiator.GetAttribute(Enum.AttrType.Cure) * (1 + add);
        }

        public float GetInspirePower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var add = GetElementAdd(initiatorID, targetID);
            return 20 * (1 + add);
        }
    }
}