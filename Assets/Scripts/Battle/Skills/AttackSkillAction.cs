using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class AttackSkillAction : SkillAction
    {
        protected List<MapObject> _arenaObjects = new List<MapObject>();

        public AttackSkillAction(int id, int initiatorID) : base(id, initiatorID)
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

        public override void Start()
        {
            EnterGrayedMode();
        }

        public override void Play()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Battle);

            ExitGrayedMode();

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.Attacking);

            CoroutineMgr.Instance.StartCoroutine(PlayAttack());
        }

        protected override IEnumerator Over(float waitingTime = 0, bool isKill = false)
        {
            yield return new WaitForSeconds(waitingTime);
            if (0 != _targetID && !_skipBattleShow)
            {
                CloseBattleArena();
            }

            //if (isKill)
            //{
            //    RoleManager.Instance.RemoveRole(_targetID);
            //}

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            _initiatorID = 0;
            _targetID = 0;
            initiator.SetState(Enum.RoleState.Over);
            //initiator.SetGrayed(true);

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Skill_Over);
        }

        public override void Dispose()
        {
            ExitGrayedMode();
            base.Dispose();
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            var initiator = RoleManager.Instance.GetRole(sender);
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
                    var hurt = AttributeMgr.Instance.GetAttackPower(_initiatorID, _targetID);
                    target.Hit(hurt, initiator.GetAttackEffect(), initiator.Hexagon);
                    target.AddBuffs(initiator.GetAttackBuffs());
                    CameraMgr.Instance.ShakePosition();
                }
            }
        }

        protected virtual IEnumerator PlayAttack()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var target = RoleManager.Instance.GetRole(_targetID);

            var initiatorForward = target.GetPosition() - initiator.GetPosition();
            initiatorForward.y = 0;
            initiator.SetForward(initiatorForward);

            if (!_skipBattleShow)
            {
                yield return OpenBattleArena(initiator, target);
            }

            //yield return new WaitForSeconds(1.0f);
            initiator.Attack(target.GetEffectPos());
            //var skillConfig = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _id);
            ////判断是攻击还是治疗
            ////执行攻击或治疗
            ////如果buff生效，添加buff到目标身上
            //DebugManager.Instance.Log(skillConfig.AttrType);
            //switch (skillConfig.AttrType)
            //{
            //    case Enum.AttrType.PhysicalAttack:
            //        initiator.Attack(target.GetPosition() + new Vector3(0, 0.6F, 0));
            //        break;
            //    case Enum.AttrType.Cure:
            //        initiator.Cure();
            //        break;
            //}
        }

        protected virtual void OnAttackedEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            var target = RoleManager.Instance.GetRole(targetID);
            if (target.IsDead())
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        protected virtual void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F, true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        protected virtual void OnDodgeEnd(object[] args)
        {
            DebugManager.Instance.Log("OnDodgeEnd");
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        protected virtual IEnumerator OpenBattleArena(Role initiator, Role target)
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(false);
            }

            LockCamera();
            CameraMgr.Instance.OpenBattleArena();
            var moveDuration = 0.2F;

            var camForward = CameraMgr.Instance.GetMainCamForward();
            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + camForward * 10;
            var initiatorToTargetDis = Vector3.Distance(target.GetPosition(), initiator.GetPosition());
            var rightDir = CameraMgr.Instance.GetMainCamRight();
            var initiatorPos = arenaCenter - rightDir * initiatorToTargetDis / 2;
            var targetPos = arenaCenter + rightDir * initiatorToTargetDis / 2;
            var hexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);
            hexagon.SetForward(camForward - new Vector3(0, camForward.y, 0));
            hexagon.ChangeToArenaSpace(arenaCenter - rightDir * initiatorToTargetDis / 2 - CommonParams.Offset, moveDuration);
            _arenaObjects.Add(hexagon);

            initiator.SetForward(targetPos - initiatorPos);
            initiator.ChangeToArenaSpace(initiatorPos, moveDuration);
            _arenaObjects.Add(initiator);

            yield return new WaitForSeconds(moveDuration);

            hexagon = MapManager.Instance.GetHexagon(target.Hexagon);
            hexagon.SetForward(camForward - new Vector3(0, camForward.y, 0));
            hexagon.ChangeToArenaSpace(arenaCenter + rightDir * initiatorToTargetDis / 2 - CommonParams.Offset, moveDuration);
            _arenaObjects.Add(hexagon);

            target.SetForward(initiatorPos - targetPos);
            target.ChangeToArenaSpace(targetPos, moveDuration);
            _arenaObjects.Add(target);
            yield return new WaitForSeconds(moveDuration);

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, new List<int> { _targetID } });
            yield return new WaitForSeconds(1);
        }

        protected virtual void CloseBattleArena()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Close_HP);

            foreach (var v in _arenaObjects)
            {
                v.ChangeToMapSpace();
            }
            _arenaObjects.Clear();

            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(true);
            }

            CameraMgr.Instance.CloseBattleArena();
            UnlockCamera();
        }

        public override void ClickHero(int id)
        {
            if (!IsTarget(Enum.RoleType.Hero))
                return;
            var initiatorID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetID = RoleManager.Instance.GetHexagonIDByRoleID(id);

            List<string> hexagons = MapManager.Instance.FindingAttackPathForStr(initiatorID, targetID, RoleManager.Instance.GetRole(_initiatorID).GetAttackDis());
            if (null == hexagons)
                return;
            _targetID = id;
            Play();
        }

        public override void ClickEnemy(int id)
        {
            if (!IsTarget(Enum.RoleType.Enemy))
                return;
            var initiatorID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetID = RoleManager.Instance.GetHexagonIDByRoleID(id);
            List<string> hexagons = MapManager.Instance.FindingAttackPathForStr(initiatorID, targetID, RoleManager.Instance.GetRole(_initiatorID).GetAttackDis());
            if (null == hexagons )
                return;
            _targetID = id;
            Play();
        }
    }
}
