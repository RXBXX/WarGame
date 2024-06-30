/// <summary>
/// 群体技能基类
/// </summary>
namespace WarGame
{
    public class MassSkill : Skill
    {
        public MassSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        public override void Start()
        {
            var roles = RoleManager.Instance.GetAllRoles();
            foreach (var v in roles)
            {
                if (v.ID == _initiatorID)
                    continue;

                if (IsTarget(v.Type))
                    _targets.Add(v.ID);
            }

            Play();
        }
    }
}
