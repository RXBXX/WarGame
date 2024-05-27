using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    public class GroupAttackSkillAction : AttackSkillAction
    {
        private Dictionary<int, LineRenderer> _chainsDic = new Dictionary<int, LineRenderer>();
        private int _chainMatID;
        private List<int> _targets = new List<int>();

        public GroupAttackSkillAction(int id, int initiatorID) : base(id, initiatorID)
        {

        }

        public override void Dispose()
        {
            AssetMgr.Instance.ReleaseAsset(_chainMatID);

            ClearChains();

            base.Dispose();
        }

        protected override IEnumerator OpenBattleArena(Role initiator, Role target)
        {
            DebugManager.Instance.Log("OpenBattleArena");
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(false);
            }
            CameraMgr.Instance.Lock();
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
                            _chainsDic.Add(neighbouringRole.ID, neighbouringRole.GameObject.AddComponent<LineRenderer>());
                            _targets.Add(neighbouringRoleID);

                            var neighbouringHexagon = MapManager.Instance.GetHexagon(hexagonKey);
                            neighbouringHexagon.ChangeToArenaSpace(neighbouringHexagon.GetPosition() + deltaVec, moveDuration);
                            _arenaObjects.Add(neighbouringHexagon);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(moveDuration);

            _chainMatID = AssetMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/ChainMat.mat", (Material mat) =>
            {
                foreach (var v in _chainsDic)
                {
                    v.Value.material = mat;
                    v.Value.startWidth = 0.08f;
                    v.Value.endWidth = 0.08f;
                    v.Value.textureScale = new Vector2(8, 1);
                    v.Value.SetPositions(new Vector3[] { target.GetEffectPos(), RoleManager.Instance.GetRole(v.Key).GetEffectPos()});
                }
            });

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_HP, new object[] { _initiatorID, _targetID });
            yield return new WaitForSeconds(1);
        }

        protected override void CloseBattleArena()
        {
            ClearChains();
            base.CloseBattleArena();
        }

        protected override void OnAttackedEnd(object[] args)
        {
            var targetID = (int)args[0];
            if(!_targets.Contains(targetID))
                return;

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
                AssetMgr.Instance.Destroy(v.Value);
            }
            _chainsDic.Clear();
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            var initiator = RoleManager.Instance.GetRole(sender);
            if ("Attack" == stateName && "Take" == secondStateName)
            {
                var attackPower = initiator.GetAttackPower();
                foreach (var v in _targets)
                {
                    var target = RoleManager.Instance.GetRole(v);
                    if (v == _targetID)
                    {
                        target.Hit(attackPower);
                        target.AddBuffs(initiator.GetAttackBuffs());
                    }
                    else
                    {
                        target.Hit(attackPower * 0.6F);
                    }
                }
                CameraMgr.Instance.ShakePosition();

                //var target = RoleManager.Instance.GetRole(_targetID);
                //var skillConfig = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _id);
                ////判断是攻击还是治疗
                ////执行攻击或治疗
                ////如果buff生效，添加buff到目标身上
                //switch (skillConfig.AttrType)
                //{
                //    case Enum.AttrType.PhysicalAttack:
                //        var attackPower = owner.GetAttackPower();
                //        DebugManager.Instance.Log("AttackPower:" + attackPower);
                //        target.Hit(attackPower);
                //        target.AddBuffs(owner.GetAttackBuffs());
                //        CameraMgr.Instance.ShakePosition();
                //        break;
                //    case Enum.AttrType.Cure:
                //        var curePower = owner.GetCurePower();
                //        target.Cured(curePower);
                //        break;
                //}
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_HP_Change, new object[] { _targetID });
            }
        }
    }
}
