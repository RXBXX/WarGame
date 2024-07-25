using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class FierceAttackSkill : SingleSkill
    {

        public FierceAttackSkill(int id, int initiatorID) : base(id, initiatorID)
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
                BattleMgr.Instance.DoAttack(_initiatorID, _targets);
                //BattleMgr.Instance.DoAttack(_initiatorID, _targets[0]);
            }
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

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnDeadEnd(object[] args)
        {
            var sender = (int)args[0];
            if (!_targets.Contains(sender))
                return;

            _targets.Remove(sender);

            if (_targets.Count > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F, true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnDodgeEnd(object[] args)
        {
            var sender = (int)args[0];
            if (!_targets.Contains(sender))
                return;

            _targets.Remove(sender);

            if (_targets.Count > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
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

        protected override void Preview(int touchingID)
        {
            DebugManager.Instance.Log("Preview");
            _previewTargets = FindTargets(touchingID);
            foreach (var v in _previewTargets)
            {
                var role = RoleManager.Instance.GetRole(v);
                if (!_attackableTargets.Contains(v))
                {
                    role.SetLayer(Enum.Layer.Gray);
                    role.SetHUDRoleVisible(true);
                }
                role.Preview(BattleMgr.Instance.GetAttackValue(_initiatorID, v));
            }
        }

        protected override void CancelPreview()
        {
            DebugManager.Instance.Log("CancelPreview");
            foreach (var v in _previewTargets)
            {
                var role = RoleManager.Instance.GetRole(v);
                if (!_attackableTargets.Contains(v))
                {
                    role.RecoverLayer();
                    role.SetHUDRoleVisible(false);
                }
                role.CancelPreview();
            }
            _previewTargets.Clear();
        }
    }
}
