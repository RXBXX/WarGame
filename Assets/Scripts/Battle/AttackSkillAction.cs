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
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Dead_End, OnDeadEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Dead_End, OnDeadEnd);
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

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Skill_Over);
        }

        public override void Dispose()
        {
            ExitGrayedMode();
            RemoveListeners();
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            var initiator = RoleManager.Instance.GetRole(sender);
            if ("Attack" == stateName && "Take" == secondStateName)
            {
                var attackPower = initiator.GetAttackPower();
                var target = RoleManager.Instance.GetRole(_targetID);
                target.Hit(attackPower);
                target.AddBuffs(initiator.GetAttackBuffs());
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

        private IEnumerator PlayAttack()
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

            yield return new WaitForSeconds(1.0f);
            initiator.Attack(target.GetPosition() + new Vector3(0, 0.6F, 0));
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

        private void EnterGrayedMode()
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
                if (IsTarget(roles[i].Type) && roles[i].ID != _initiatorID && regionDic.ContainsKey(roles[i].hexagonID))
                {
                    roles[i].SetLayer(8);
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

        private void ExitGrayedMode()
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

        private void OnAttackedEnd(object[] args)
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

        private void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F, true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private IEnumerator OpenBattleArena(Role initiator, Role target)
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(false);
            }
            CameraMgr.Instance.OpenGray();

            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + CameraMgr.Instance.GetMainCamForward() * 5;
            var pathCenter = (target.GetPosition() + initiator.GetPosition()) / 2.0F;
            var deltaVec = arenaCenter - pathCenter;
            var path = MapManager.Instance.FindingPathForStr(initiator.hexagonID, target.hexagonID, Enum.RoleType.Hero, false);

            var moveDuration = 0.2F;
            for (int i = 0; i < path.Count; i++)
            {
                if (0 == i)
                {
                    initiator.ChangeToArenaSpace(initiator.GetPosition() + deltaVec, moveDuration);
                    initiator.SetLayer(8);
                    _arenaObjects.Add(initiator);
                }
                else if (path.Count - 1 == i)
                {
                    target.ChangeToArenaSpace(target.GetPosition() + deltaVec, moveDuration);
                    target.SetLayer(8);
                    _arenaObjects.Add(target);
                }
                var hexagon = MapManager.Instance.GetHexagon(path[i]);
                hexagon.ChangeToArenaSpace(hexagon.GetPosition() + deltaVec, moveDuration);
                hexagon.SetLayer(8);
                _arenaObjects.Add(hexagon);
                yield return new WaitForSeconds(moveDuration);
            }

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_HP, new object[] { _initiatorID, _targetID });
        }

        private void CloseBattleArena()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Close_HP);

            foreach (var v in _arenaObjects)
            {
                v.ChangeToMapSpace();
                v.RecoverLayer();
            }
            _arenaObjects.Clear();

            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(true);
            }

            CameraMgr.Instance.CloseGray();
        }

        public override void ClickHero(int id)
        {
            if (!IsTarget(Enum.RoleType.Hero))
                return;

            var initiatorID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetID = RoleManager.Instance.GetHexagonIDByRoleID(id);
            List<string> hexagons = MapManager.Instance.FindingPathForStr(initiatorID, targetID, Enum.RoleType.Hero, false);
            if (null == hexagons || hexagons.Count <= 0)
                return;

            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }

            var hero = RoleManager.Instance.GetRole(_initiatorID);
            if (cost > hero.GetAttackDis())
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
            List<string> hexagons = MapManager.Instance.FindingPathForStr(initiatorID, targetID, Enum.RoleType.Hero, false);
            if (null == hexagons || hexagons.Count <= 0)
                return;
            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }

            var hero = RoleManager.Instance.GetRole(_initiatorID);
            if (cost > hero.GetAttackDis())
                return;

            _targetID = id;
            Play();
        }
    }
}
