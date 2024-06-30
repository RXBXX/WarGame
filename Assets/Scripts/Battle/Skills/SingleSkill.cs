///单体技能基类
using System.Collections.Generic;

namespace WarGame
{
    public class SingleSkill : Skill
    {
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

            var initiatorID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetID = RoleManager.Instance.GetHexagonIDByRoleID(id);
            List<string> hexagons = MapManager.Instance.FindingAttackPathForStr(initiatorID, targetID, RoleManager.Instance.GetRole(_initiatorID).GetAttackDis());
            if (null == hexagons)
                return;

            _targets.Add(id);
            Play();
        }

        public override void ClickEnemy(int id)
        {
            if (!IsTarget(Enum.RoleType.Enemy))
                return;

            var initiatorID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetID = RoleManager.Instance.GetHexagonIDByRoleID(id);
            List<string> hexagons = MapManager.Instance.FindingAttackPathForStr(initiatorID, targetID, RoleManager.Instance.GetRole(_initiatorID).GetAttackDis());
            if (null == hexagons)
                return;

            _targets.Add(id);
            Play();
        }

        protected virtual void EnterGrayedMode()
        {
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var regionDic = MapManager.Instance.FindingAttackRegion(new List<string> { hexagonID }, initiator.GetAttackDis());

            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                if (IsTarget(roles[i].Type) && roles[i].ID != _initiatorID && regionDic.ContainsKey(roles[i].Hexagon))
                {
                    roles[i].SetLayer(Enum.Layer.Gray);
                }
                else
                {
                    roles[i].SetHPVisible(false);
                    roles[i].SetColliderEnable(false);
                }
            }
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
                roles[i].SetHPVisible(true);
            }
            CameraMgr.Instance.CloseGray();
        }
    }
}
