using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    public class GroupAttackSkillAction : AttackSkillAction
    {
        private int _chainMatID;
        private Dictionary<int, Chain> _chainsDic = new Dictionary<int, Chain>();
        private List<int> _targets = new List<int>();
        private float _hurt;

        public GroupAttackSkillAction(int id, int initiatorID) : base(id, initiatorID)
        {

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

        protected override IEnumerator OpenBattleArena(Role initiator, Role target)
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(false);
            }
            LockCamera();
            CameraMgr.Instance.OpenGray();

            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + CameraMgr.Instance.GetMainCamForward() * 10;
            var pathCenter = (target.GetPosition() + initiator.GetPosition()) / 2.0F;
            var deltaVec = arenaCenter - pathCenter;

            var moveDuration = 0.2F;

            var hexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);
            hexagon.ChangeToArenaSpace(hexagon.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(hexagon);

            initiator.ChangeToArenaSpace(initiator.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(initiator);

            yield return new WaitForSeconds(moveDuration);

            hexagon = MapManager.Instance.GetHexagon(target.Hexagon);
            hexagon.ChangeToArenaSpace(hexagon.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(hexagon);

            target.ChangeToArenaSpace(target.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(target);

            _targets.Add(_targetID);

            var directions = MapManager.Instance.Dicections;
            for (int i = 0; i < directions.Length; i++)
            {
                var hexagonKey = MapTool.Instance.GetHexagonKey(hexagon.coor + directions[i]);
                if (MapManager.Instance.ContainHexagon(hexagonKey))
                {
                    var neighbouringRoleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonKey);
                    if (0 != neighbouringRoleID)
                    {
                        var neighbouringRole = RoleManager.Instance.GetRole(neighbouringRoleID);
                        if (IsTarget(neighbouringRole.Type))
                        {
                            neighbouringRole.ChangeToArenaSpace(neighbouringRole.GetPosition() + deltaVec, moveDuration);
                            _arenaObjects.Add(neighbouringRole);
                            _targets.Add(neighbouringRoleID);

                            var neighbouringHexagon = MapManager.Instance.GetHexagon(hexagonKey);
                            neighbouringHexagon.ChangeToArenaSpace(neighbouringHexagon.GetPosition() + deltaVec, moveDuration);
                            _arenaObjects.Add(neighbouringHexagon);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(moveDuration);

            _chainMatID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/ChainMat.mat", (Material mat) =>
            {
                foreach (var v in _targets)
                {
                    if (v != _targetID)
                        _chainsDic.Add(v, new Chain(target.GetEffectPoint(), RoleManager.Instance.GetRole(v).GetEffectPoint(), mat));
                }
            });

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, _targets });
            yield return new WaitForSeconds(1.5F);
        }

        protected override void CloseBattleArena()
        {
            ClearChains();
            base.CloseBattleArena();
        }

        protected override void OnAttackedEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (!_targets.Contains(targetID))
                return;

            if (targetID == _targetID)
            {
                foreach (var v in _targets)
                {
                    if (v != _targetID)
                    {
                        RoleManager.Instance.GetRole(v).Hit(_hurt * 0.5F);
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

        protected override void OnDeadEnd(object[] args)
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


        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            if ("Attack" == stateName && "Take" == secondStateName)
            {
                var target = RoleManager.Instance.GetRole(_targetID);
                var dodgeRatio = target.GetAttribute(Enum.AttrType.DodgeRatio);
                var rd = Random.Range(0, 1.0f);
                if (rd < dodgeRatio)
                {
                    target.Dodge();
                }
                else
                {
                    _hurt = AttributeMgr.Instance.GetAttackPower(_initiatorID, _targetID);
                    target.Hit(_hurt);
                    target.AddBuffs(initiator.GetAttackBuffs());
                    CameraMgr.Instance.ShakePosition();
                }
            }
        }
    }
}
