using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    public class ChainAttackSkill : SingleSkill
    {
        private int _chainMatID;
        private Dictionary<int, Chain> _chainsDic = new Dictionary<int, Chain>();
        private Dictionary<int, List<int>> _chainTargetDic = new Dictionary<int, List<int>>();

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
                BattleMgr.Instance.DoChainAttack(_initiatorID, _targets);
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

            foreach (var v in _targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                var attackRegion = MapManager.Instance.FindingAttackRegion(new List<int> { target.Hexagon }, 1);
                foreach (var v1 in attackRegion)
                {
                    if (v1.Key == target.Hexagon)
                        continue;

                    var roleID = RoleManager.Instance.GetRoleIDByHexagonID(v1.Key);
                    if (0 == roleID)
                        continue;

                    if (_targets.Contains(roleID))
                        continue;

                    var role = RoleManager.Instance.GetRole(roleID);
                    if (!IsTarget(role.Type))
                        continue;

                    var dirty = false;
                    foreach (var v2 in _chainTargetDic)
                    {
                        if (v2.Value.Contains(roleID))
                        {
                            dirty = true;
                            break;
                        }
                    }

                    if (dirty)
                        continue;

                    if (!_chainTargetDic.ContainsKey(v))
                    {
                        _chainTargetDic.Add(v, new List<int>());
                    }
                    _chainTargetDic[v].Add(roleID);
                }

                foreach (var v1 in attackRegion)
                    v1.Value.Recycle();
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
            var roleCount = 1;
            var pathCenter = initiator.GetPosition();
            foreach (var v in _targets)
            {
                pathCenter += RoleManager.Instance.GetRole(v).GetPosition();
                roleCount++;
            }
            foreach (var v in _chainTargetDic)
            {
                foreach (var v1 in v.Value)
                {
                    pathCenter += RoleManager.Instance.GetRole(v1).GetPosition();
                    roleCount++;
                }
            }
            pathCenter /= roleCount;

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
            foreach (var v in _chainTargetDic)
            {
                foreach (var v1 in v.Value)
                {
                    yield return new WaitForSeconds(moveDuration);
                    var target = RoleManager.Instance.GetRole(v1);
                    hexagon = MapManager.Instance.GetHexagon(target.Hexagon);
                    hexagon.ChangeToArenaSpace(hexagon.GetPosition() + deltaVec, moveDuration);
                    _arenaObjects.Add(hexagon);

                    target.ChangeToArenaSpace(target.GetPosition() + deltaVec, moveDuration);
                    _arenaObjects.Add(target);
                }
            }

            yield return new WaitForSeconds(moveDuration);

            _chainMatID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/ChainMat.mat", (Material mat) =>
            {
                foreach (var v in _chainTargetDic)
                {
                    var origon = RoleManager.Instance.GetRole(v.Key).GetEffectPoint();
                    foreach (var v1 in v.Value)
                    {
                        _chainsDic.Add(v1, new Chain(origon, RoleManager.Instance.GetRole(v1).GetEffectPoint(), mat));
                    }
                }
            });

            var skillName = GetConfig().GetTranslation("Name");
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, _targets, skillName });
            yield return new WaitForSeconds(1.5F);
        }

        protected override void CloseBattleArena()
        {
            ClearChains();
            base.CloseBattleArena();
        }

        private void OnAttackedEnd(object[] args)
        {
            var sender = (int)args[0];
            var target = RoleManager.Instance.GetRole(sender);

            if (_targets.Contains(sender))
            {
                if (_chainTargetDic.ContainsKey(sender))
                {
                    BattleMgr.Instance.DoChainAttack(_initiatorID, _chainTargetDic[sender]);
                }

                if (target.IsDead())
                    return;

                _targets.Remove(sender);
            }
            else
            {
                if (target.IsDead())
                    return;

                foreach (var v in _chainTargetDic)
                {
                    if (v.Value.Contains(sender))
                    {
                        v.Value.Remove(sender);
                        if (v.Value.Count <= 0)
                            _chainTargetDic.Remove(v.Key);
                        break;
                    }
                }
            }

            if (_targets.Count > 0)
                return;

            if (_chainTargetDic.Count > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over(1.5F));
        }

        private void OnDeadEnd(object[] args)
        {
            var sender = (int)args[0];

            if (_targets.Contains(sender))
            {
                _targets.Remove(sender);
            }
            else
            {
                foreach (var v in _chainTargetDic)
                {
                    if (v.Value.Contains(sender))
                    {
                        v.Value.Remove(sender);
                        if (v.Value.Count <= 0)
                            _chainTargetDic.Remove(v.Key);
                        break;
                    }
                }
            }

            if (_targets.Count > 0)
                return;

            if (_chainTargetDic.Count > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over(1.5F));
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
