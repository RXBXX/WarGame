using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class AttackAndRelocateSkill : SingleSkill
    {
        private int _stage = 0;

        public AttackAndRelocateSkill(int id, int initiatorID, int levelID) : base(id, initiatorID, levelID)
        {

        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Dead_End, OnDeadEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Dead_End, OnDeadEnd);
        }

        protected override void TriggerSkill()
        {
            base.TriggerSkill();
            var hitPoss = new List<Vector3>();
            foreach (var v in _targets)
            {
                hitPoss.Add(RoleManager.Instance.GetRole(v).GetHitPos());
            }
            RoleManager.Instance.GetRole(_initiatorID).Attack(hitPoss);
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Attack" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoRelocateAttack(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).Element, _initiatorID, _targets);
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

        public override void ClickHexagon(int id)
        {
            if (1 != _stage)
                return;
            var start = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var end = id;

            var hexagons = MapManager.Instance.FindingPathForStr(start, end, RoleManager.Instance.GetRole(_initiatorID).GetMoveDis(), Enum.RoleType.Hero);

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
            var sender = (int)args[0];
            if (!_targets.Contains(sender))
                return;

            var target = RoleManager.Instance.GetRole(sender);
            if (target.IsDead())
                return;

            _targets.Remove(sender);
            if (_targets.Count > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(OnStageOver(1.5F));
        }

        private void OnDeadEnd(object[] args)
        {
            var sender = (int)args[0];
            if (!_targets.Contains(sender))
                return;

            _targets.Remove(sender);

            if (_targets.Count > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(OnStageOver(1.5F));
        }


        public override void OnMoveEnd()
        {
            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over());
        }

        private IEnumerator OnStageOver(float waitingTime = 0)
        {
            yield return new WaitForSeconds(waitingTime);
            if (!DatasMgr.Instance.GetSkipBattle())
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
