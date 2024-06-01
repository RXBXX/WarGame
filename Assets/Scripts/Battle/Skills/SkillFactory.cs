using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class SkillFactory : Singeton<SkillFactory>
    {
        public SkillAction GetSkill(int skillID, int initiatorID)
        {
            switch((Enum.Skill)skillID)
            {
                case Enum.Skill.Attack:
                    return new AttackSkillAction(skillID, initiatorID);
                case Enum.Skill.Cure:
                    return new CureSkillAction(skillID, initiatorID);
                case Enum.Skill.AttackRetreat:
                    return new AttackRetreatSkillAction(skillID, initiatorID);
                case Enum.Skill.GroupAttack:
                    return new GroupAttackSkillAction(skillID, initiatorID);
            }
            return null;
        }
    }
}
