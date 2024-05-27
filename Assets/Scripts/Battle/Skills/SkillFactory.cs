using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class SkillFactory : Singeton<SkillFactory>
    {
        public SkillAction GetSkill(int skillID, int initiatorID)
        {
            switch((Enum.SkillType)skillID)
            {
                case Enum.SkillType.Attack:
                    return new AttackSkillAction(skillID, initiatorID);
                case Enum.SkillType.Cure:
                    return new CureSkillAction(skillID, initiatorID);
                case Enum.SkillType.AttackRetreat:
                    return new AttackRetreatSkillAction(skillID, initiatorID);
                case Enum.SkillType.GroupAttack:
                    return new GroupAttackSkillAction(skillID, initiatorID);
            }
            return null;
        }
    }
}
