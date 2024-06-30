using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class AttackAndRelocateSkill : FierceAttackSkill
    {
        private int _stage = 0;

        public AttackAndRelocateSkill(int id, int initiatorID) : base(id, initiatorID)
        {

        }

        public override void ClickHero(int id)
        {
            if (0 != _stage)
                return;

            base.ClickHero(id);
        }

        public override void ClickEnemy(int id)
        {
            if (0 != _stage)
                return;

            base.ClickEnemy(id);
        }

        public override void ClickHexagon(string id)
        {
            if (1 != _stage)
                return;
            var start = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var end = id;

            List<string> hexagons = MapManager.Instance.FindingPathForStr(start, end, RoleManager.Instance.GetRole(_initiatorID).GetMoveDis(), Enum.RoleType.Hero);

            if (null == hexagons || hexagons.Count <= 0)
                return;
            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }
            var hero = RoleManager.Instance.GetRole(_initiatorID);
            if (cost > hero.GetMoveDis())
                return;
            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.Moving);

            //MapManager.Instance.ClearMarkedPath();
            MapManager.Instance.ClearMarkedRegion();

            hero.Move(hexagons);
           
        }

        protected override void OnAttackedEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            var target = RoleManager.Instance.GetRole(targetID);
            if (target.IsDead())
                return;

            if (null != _coroutine)
                return;

            _coroutine = OnStageOver(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        protected override void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            if (null != _coroutine)
                return;

            _coroutine = OnStageOver(1.5F, true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        public override void OnMoveEnd()
        {
            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private IEnumerator OnStageOver(float waitingTime = 0, bool isKill = false)
        {
            yield return new WaitForSeconds(waitingTime);
            if (0 != _targetID && !_skipBattleShow)
            {
                CloseBattleArena();
            }

            _stage += 1;

            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.Waiting);
            MapManager.Instance.MarkingRegion(role.Hexagon, role.GetMoveDis(), 0, role.Type);
        }

        protected override IEnumerator Over(float waitingTime = 0, bool isKill = false)
        {
            yield return new WaitForSeconds(waitingTime);
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            _initiatorID = 0;
            _targetID = 0;
            initiator.SetState(Enum.RoleState.Over);

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Skill_Over);
        }

    }
}
