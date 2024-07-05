using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class AttackAndRelocateSkill : SingleSkill
    {
        private int _stage = 0;

        public AttackAndRelocateSkill(int id, int initiatorID) : base(id, initiatorID)
        {

        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Dodge_End, OnDodgeEnd);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Dead_End, OnDeadEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Dead_End, OnDeadEnd);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Dodge_End, OnDodgeEnd);
        }

        protected override void TriggerSkill()
        {
            base.TriggerSkill();
            RoleManager.Instance.GetRole(_initiatorID).Attack(RoleManager.Instance.GetRole(_targets[0]).GetEffectPos());
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Attack" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoAttack(_initiatorID, _targets[0]);
            }
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

        private void OnAttackedEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targets[0])
                return;

            var target = RoleManager.Instance.GetRole(targetID);
            if (target.IsDead())
                return;

            if (null != _coroutine)
                return;

            _coroutine = OnStageOver(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targets[0])
                return;

            if (null != _coroutine)
                return;

            _coroutine = OnStageOver(1.5F, true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnDodgeEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targets[0])
                return;

            if (null != _coroutine)
                return;

            _coroutine = OnStageOver(1.5F);
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
            if (0 != _targets[0] && !DatasMgr.Instance.GetSkipBattle())
            {
                CloseBattleArena();
            }

            _stage += 1;

            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.Waiting);
            MapManager.Instance.MarkingRegion(role.Hexagon, role.GetMoveDis(), 0, role.Type);
        }
    }
}
