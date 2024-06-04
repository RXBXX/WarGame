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
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Dodge_End, OnDodgeEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Dead_End, OnDeadEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Dead_End, OnDeadEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Dodge_End, OnDodgeEnd);
        }

        public override void Start()
        {
            EnterGrayedMode();
        }

        public override void Play()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Battle);

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

            if (isKill)
            {
                RoleManager.Instance.RemoveRole(_targetID);
            }

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            _initiatorID = 0;
            _targetID = 0;
            initiator.SetState(Enum.RoleState.Over);
            //initiator.SetGrayed(true);

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Skill_Over);
        }

        public override void Dispose()
        {
            ExitGrayedMode();
            CameraMgr.Instance.UnlockTarget();
            RemoveListeners();
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
                    var add = GetElementAdd(_targetID);
                    var initiatorPhysicalAttack = initiator.GetAttribute(Enum.AttrType.PhysicalAttack) * (1+add);
                    var initiatorPhysicalAttackRatio = initiator.GetAttribute(Enum.AttrType.PhysicalAttackRatio) * (1 + add);
                    var initiatorMagicAttack = initiator.GetAttribute(Enum.AttrType.MagicAttack) * (1 + add);
                    var initiatorMagicAttackRatio = initiator.GetAttribute(Enum.AttrType.MagicAttackRatio) * (1 + add);
                    var initiatorPhysicalPenetrateRatio = initiator.GetAttribute(Enum.AttrType.PhysicalPenetrateRatio) * (1 + add);
                    var initiatorMagicPenetrateRatio = initiator.GetAttribute(Enum.AttrType.MagicPenetrateRatio) * (1 + add);

                    var targetPhysicalDefense = target.GetAttribute(Enum.AttrType.PhysicalDefense);
                    var targetMagicDefense = target.GetAttribute(Enum.AttrType.MagicDefense);

                    var physicalHurt = initiatorPhysicalAttack * (1 + initiatorPhysicalAttackRatio) - (1 - initiatorPhysicalPenetrateRatio) * targetPhysicalDefense;
                    var magicHurt = initiatorMagicAttack * (1 + initiatorMagicAttackRatio) - (1 - initiatorMagicPenetrateRatio) * targetMagicDefense;
                    target.Hit(physicalHurt + magicHurt);
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

        protected virtual void EnterGrayedMode()
        {
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var region = MapManager.Instance.FindingRegion(hexagonID, 0, initiator.GetAttackDis(), Enum.RoleType.Hero);
            var regionDic = new Dictionary<string, bool>();
            foreach (var v in region)
                regionDic[v.id] = true;

            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                if (IsTarget(roles[i].Type) && roles[i].ID != _initiatorID && regionDic.ContainsKey(roles[i].Hexagon))
                {
                    roles[i].SetLayer(Enum.Layer.Gray);
                }
                else
                {
                    roles[i].SetColliderEnable(false);
                }

                if (roles[i].ID != _initiatorID && roles[i].ID != _targetID)
                    roles[i].SetHPVisible(false);
            }
            initiator.SetState(Enum.RoleState.WatingTarget);

            CameraMgr.Instance.OpenGray();
        }

        protected virtual void ExitGrayedMode()
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].RecoverLayer();
                roles[i].SetColliderEnable(true);

                if (roles[i].ID != _initiatorID && roles[i].ID != _targetID)
                    roles[i].SetHPVisible(true);
            }

            CameraMgr.Instance.CloseGray();
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
            yield return new WaitForSeconds(moveDuration);

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, new List<int> { _targetID } });
            yield return new WaitForSeconds(1);
        }

        protected virtual void CloseBattleArena()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Close_HP);

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

            CameraMgr.Instance.CloseGray();
            CameraMgr.Instance.Unlock();
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
