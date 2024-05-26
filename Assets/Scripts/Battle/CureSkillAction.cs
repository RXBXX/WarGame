using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class CureSkillAction : SkillAction
    {
        protected List<MapObject> _arenaObjects = new List<MapObject>();

        public CureSkillAction(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Cured_End, OnCuredEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Cured_End, OnCuredEnd);
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
            RemoveListeners();
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            var initiator = RoleManager.Instance.GetRole(sender);
            if ("Cure" == stateName && "Take" == secondStateName)
            {
                var attackPower = initiator.GetAttackPower();
                var target = RoleManager.Instance.GetRole(_targetID);
                target.Cured(attackPower);
                target.AddBuffs(initiator.GetAttackBuffs());
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
            initiator.Cure();
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
                if (IsTarget(roles[i].Type) && roles[i].ID != _initiatorID && regionDic.ContainsKey(roles[i].Hexagon))
                    roles[i].SetLayer(8);
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

        private void OnCuredEnd(object[] args)
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

        private IEnumerator OpenBattleArena(Role initiator, Role target)
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(false);
            }
            CameraMgr.Instance.Lock();
            CameraMgr.Instance.OpenGray();

            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + CameraMgr.Instance.GetMainCamForward() * 5;
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

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_HP, new object[] { _initiatorID, _targetID });
        }

        private void CloseBattleArena()
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
            if (null == hexagons)
                return;

            _targetID = id;
            Play();
        }
    }
}
