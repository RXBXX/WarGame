using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class FierceAttackSkill : SingleSkill
    {
        private int _touchingID;

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

        public override void FocusIn(GameObject obj)
        {
            var touchingID = 0;
            if (null != obj)
            {
                var tag = obj.tag;
                if (tag == Enum.Tag.Hero.ToString())
                {
                    touchingID = obj.GetComponent<RoleBehaviour>().ID;
                }
                else if (tag == Enum.Tag.Enemy.ToString())
                {
                    touchingID = obj.GetComponent<RoleBehaviour>().ID;
                }
            }

            if (touchingID != _touchingID)
            {
                if (0 != _touchingID)
                {
                    RoleManager.Instance.GetRole(_touchingID).CancelPreview();
                    _touchingID = 0;
                }
                if (0 != touchingID)
                {
                    _touchingID = touchingID;
                    var hurt = BattleMgr.Instance.GetAttackValue(_initiatorID, touchingID);
                    RoleManager.Instance.GetRole(_touchingID).Preview(hurt);
                }
            }
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
                    _targets.Add(roleID);
                }

                foreach (var v in attackRegion)
                    v.Value.Recycle();
            }
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
                    _targets.Add(roleID);
                }

                foreach (var v in attackRegion)
                    v.Value.Recycle();
            }

            Play();
        }
    }
}
