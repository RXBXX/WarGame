using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class RouletteSkill : SingleSkill
    {
        public RouletteSkill(int id, int initiatorID, int levelID) : base(id, initiatorID, levelID)
        {
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            //var initiator = RoleManager.Instance.GetRole(sender);
            if ("Attack" == stateName && "Take" == secondStateName)
            {
                if (_targets.Count > 0)
                    BattleMgr.Instance.DoRoulette(_initiatorID, _targets[0]);//bug ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection.
            }
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

        private void OnAttackedEnd(object[] args)
        {
            var sender = (int)args[0];
            if (!_targets.Contains(sender) && _initiatorID != sender)
                return;

            var target = RoleManager.Instance.GetRole(sender);
            if (target.IsDead())
                return;

            if (_targets.Contains(sender))
            {
                _targets.Remove(sender);
            }

            if (_targets.Count > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over());
        }

        private void OnDeadEnd(object[] args)
        {
            var sender = (int)args[0];
            if (!_targets.Contains(sender) && _initiatorID != sender)
                return;

            if (_targets.Contains(sender))
                _targets.Remove(sender);

            if (_targets.Count > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over());
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

            _targets = FindTargets(id);

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
            _targets = FindTargets(id);

            Play();
        }

        protected override List<int> FindTargets(int id)
        {
            return new List<int>() { id };
        }
    }
}
