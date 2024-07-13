using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    public class ChainAttackSkill : SingleSkill
    {
        private int _chainMatID;
        private Dictionary<int, Chain> _chainsDic = new Dictionary<int, Chain>();

        public ChainAttackSkill(int id, int initiatorID) : base(id, initiatorID)
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
            RoleManager.Instance.GetRole(_initiatorID).Attack(RoleManager.Instance.GetRole(_targets[0]).GetEffectPos());
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Attack" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoChainAttack(_initiatorID, _targets[0]);
            }
        }

        public override void Update(float deltaTime)
        {
            foreach (var v in _chainsDic)
            {
                v.Value.Update(deltaTime);
            }
        }

        public override void Dispose()
        {
            AssetsMgr.Instance.ReleaseAsset(_chainMatID);
            ClearChains();
            base.Dispose();
        }

        protected override void Prepare()
        {
            base.Prepare();

            Role target = RoleManager.Instance.GetRole(_targets[0]);
            var hexagon = MapManager.Instance.GetHexagon(target.Hexagon);
            var directions = MapManager.Instance.Dicections;
            for (int i = 0; i < directions.Length; i++)
            {
                var hexagonKey = MapTool.Instance.GetHexagonKey(hexagon.coor + directions[i]);
                if (!MapManager.Instance.ContainHexagon(hexagonKey))
                    continue;

                var neighbouringRoleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonKey);
                if (0 == neighbouringRoleID)
                    continue;

                var neighbouringRole = RoleManager.Instance.GetRole(neighbouringRoleID);
                if (!IsTarget(neighbouringRole.Type))
                    continue;

                _targets.Add(neighbouringRoleID);
            }
        }

        protected override IEnumerator OpenBattleArena()
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHUDRoleVisible(false);
            }
            LockCamera();
            CameraMgr.Instance.OpenBattleArena();

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + CameraMgr.Instance.GetMainCamForward() * 7;
            var pathCenter = initiator.GetPosition();
            foreach (var v in _targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                pathCenter += target.GetPosition();
            }
            pathCenter /= _targets.Count + 1;

            var deltaVec = arenaCenter - pathCenter;

            var moveDuration = 0.2F;
            var hexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);
            hexagon.ChangeToArenaSpace(hexagon.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(hexagon);

            initiator.ChangeToArenaSpace(initiator.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(initiator);

            foreach (var v in _targets)
            {
                yield return new WaitForSeconds(moveDuration);
                var target = RoleManager.Instance.GetRole(v);
                hexagon = MapManager.Instance.GetHexagon(target.Hexagon);
                hexagon.ChangeToArenaSpace(hexagon.GetPosition() + deltaVec, moveDuration);
                _arenaObjects.Add(hexagon);

                target.ChangeToArenaSpace(target.GetPosition() + deltaVec, moveDuration);
                _arenaObjects.Add(target);
            }

            yield return new WaitForSeconds(moveDuration);

            _chainMatID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/ChainMat.mat", (Material mat) =>
            {
                foreach (var v in _targets)
                {
                    if (v != _targets[0])
                    {
                        var target = RoleManager.Instance.GetRole(_targets[0]);
                        _chainsDic.Add(v, new Chain(target.GetEffectPoint(), RoleManager.Instance.GetRole(v).GetEffectPoint(), mat));
                    }
                }
            });

            var skillName = GetConfig().Name;
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, _targets, skillName});
            yield return new WaitForSeconds(1.5F);
        }

        protected override void CloseBattleArena()
        {
            ClearChains();
            base.CloseBattleArena();
        }

        private void OnAttackedEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (!_targets.Contains(targetID))
                return;

            if (targetID == _targets[0])
            {
                for(int i = _targets.Count - 1; i >= 0; i--)
                {
                    if (i >= _targets.Count)
                        continue;
                    if (_targets[i] != _targets[0])
                    {
                        BattleMgr.Instance.DoAttack(_initiatorID, _targets[i]);
                    }
                }
            }

            var target = RoleManager.Instance.GetRole(targetID);
            if (target.IsDead())
                return;

            _targets.Remove(targetID);
            if (_targets.Count > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (!_targets.Contains(targetID))
                return;

            _targets.Remove(targetID);
            if (_targets.Count > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F, true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void ClearChains()
        {
            foreach (var v in _chainsDic)
            {
                v.Value.Dispose();
            }
            _chainsDic.Clear();
        }
    }
}
