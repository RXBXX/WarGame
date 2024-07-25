///单体技能基类
using System.Collections.Generic;

namespace WarGame
{
    public class SingleSkill : Skill
    {
        protected List<int> _attackableTargets = new List<int>();
        protected List<int> _previewTargets = new List<int>();

        public SingleSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        public override void Start()
        {
            base.Start();
            EnterGrayedMode();
        }

        public override void Dispose()
        {
            base.Dispose();
            ExitGrayedMode();
        }

        public override void Play()
        {
            ExitGrayedMode();
            base.Play();
        }

        protected override void Prepare()
        {
            base.Prepare();

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var initiatorForward = RoleManager.Instance.GetRole(_targets[0]).GetPosition() - initiator.GetPosition();
            initiatorForward.y = 0;
            initiator.SetForward(initiatorForward);
        }

        public override void ClickHero(int id)
        {
            if (!IsTarget(Enum.RoleType.Hero))
                return;

            var startHexID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetHexID = RoleManager.Instance.GetHexagonIDByRoleID(id);
            var hexagons = MapManager.Instance.FindingAttackPathForStr(startHexID, targetHexID, RoleManager.Instance.GetRole(_initiatorID).GetAttackDis());
            if (null == hexagons)
                return;

            _targets.Add(id);

            Play();
        }

        public override void ClickEnemy(int id)
        {
            if (!IsTarget(Enum.RoleType.Enemy))
                return;

            var startHexID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetHexID = RoleManager.Instance.GetHexagonIDByRoleID(id);
            var hexagons = MapManager.Instance.FindingAttackPathForStr(startHexID, targetHexID, RoleManager.Instance.GetRole(_initiatorID).GetAttackDis());
            if (null == hexagons)
                return;

            _targets.Add(id);

            Play();
        }

        protected virtual void EnterGrayedMode()
        {
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var regionDic = MapManager.Instance.FindingAttackRegion(new List<int> { hexagonID }, initiator.GetAttackDis());

            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                if (IsTarget(roles[i].Type) && roles[i].ID != _initiatorID && regionDic.ContainsKey(roles[i].Hexagon))
                {
                    _attackableTargets.Add(roles[i].ID);
                    roles[i].SetLayer(Enum.Layer.Gray);
                }
                else
                {
                    roles[i].SetHUDRoleVisible(false);
                    roles[i].SetColliderEnable(false);
                }
            }

            foreach (var v in regionDic)
                v.Value.Recycle();
            regionDic.Clear();

            initiator.SetState(Enum.RoleState.WatingTarget);
            CameraMgr.Instance.OpenGray();
        }

        protected virtual void ExitGrayedMode()
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].RecoverLayer();
                roles[i].SetColliderEnable(true);
                roles[i].SetHUDRoleVisible(true);
            }
            CameraMgr.Instance.CloseGray();

            _attackableTargets.Clear();
        }

        protected List<int> FindTargets(int selectTarget)
        {
            var targets = new List<int>();
            targets.Add(selectTarget);

            var startHexID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetHexID = RoleManager.Instance.GetHexagonIDByRoleID(selectTarget);

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var attackRange = initiator.GetAttribute(Enum.AttrType.AttackRange);

            if (attackRange > 0)
            {
                var attackRegion = MapManager.Instance.FindingAttackRegion(new List<int> { targetHexID }, attackRange);
                foreach (var v in attackRegion)
                {
                    if (v.Key == startHexID || v.Key == targetHexID)
                        continue;
                    var roleID = RoleManager.Instance.GetRoleIDByHexagonID(v.Key);
                    if (0 == roleID)
                        continue;
                    var role = RoleManager.Instance.GetRole(roleID);
                    if (!IsTarget(role.Type))
                        continue;
                    targets.Add(roleID);
                }

                foreach (var v in attackRegion)
                    v.Value.Recycle();
            }
            return targets;
        }
    }
}
