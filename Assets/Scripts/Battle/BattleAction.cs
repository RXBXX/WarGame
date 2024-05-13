using WarGame.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class BattleAction
    {
        protected int _initiatorID;
        protected int _targetID;
        protected Enum.SkillType _skillType;
        protected List<string> _path; //英雄移动的路径
        protected IEnumerator _coroutine;
        protected bool _skipBattleShow = true;
        protected List<MapObject> _arenaObjects = new List<MapObject>();
        protected string _touchingHexagon = null;

        public BattleAction()
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Attack_End, OnAttackEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Dead_End, OnDeadEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.AddListener(Enum.EventType.Role_MoveEnd_Event, OnMoveEnd);
        }

        public virtual void Dispose()
        {
            if (null != _coroutine)
            {
                CoroutineMgr.Instance.StartCoroutine(_coroutine);
                _coroutine = null;
            }

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Attack_End, OnAttackEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Dead_End, OnDeadEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Role_MoveEnd_Event, OnMoveEnd);
        }

        protected IEnumerator OpenBattleArena(Role initiator, Role target)
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

        protected void CloseBattleArena()
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

        protected void EnterGrayedMode()
        {
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var hero = RoleManager.Instance.GetRole(_initiatorID);
            var region = MapManager.Instance.FindingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
            var regionDic = new Dictionary<string, bool>();
            foreach (var v in region)
                regionDic[v.id] = true;

            var targetType = hero.GetTargetType(_skillType);
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].Type == targetType && roles[i].ID != _initiatorID && regionDic.ContainsKey(roles[i].hexagonID))
                    roles[i].SetLayer(8);
                else
                {
                    roles[i].SetColliderEnable(false);
                }

                if (roles[i].ID != _initiatorID && roles[i].ID != _targetID)
                    roles[i].SetHPVisible(false);
            }
            hero.SetState(Enum.RoleState.WatingTarget);

            CameraMgr.Instance.OpenGray();
        }

        protected void ExitGrayedMode()
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

        protected void OnAttackEnd(object[] args)
        {
            if (_initiatorID <= 0)
                return;

            if (_targetID > 0)
                return;

            if (null != _coroutine)
                return;

            DebugManager.Instance.Log("OnAttackEnd");
            _coroutine = OnFinishAction();
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
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

            DebugManager.Instance.Log("OnAttackedEnd");
            _coroutine = OnFinishAction(1.5f);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            if (null != _coroutine)
                return;

            _coroutine = OnFinishAction(1.5F, true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private IEnumerator PlayAttack()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var target = RoleManager.Instance.GetRole(_targetID);

            var initiatorForward = target.GetPosition() - initiator.GetPosition();
            initiatorForward.y = 0;
            initiator.SetForward(initiatorForward);

            var targetForward = initiator.GetPosition() - target.GetPosition();
            targetForward.y = 0;
            target.SetForward(targetForward);

            DebugManager.Instance.Log("11111" + _targetID);
            if (!_skipBattleShow)
            {
                yield return OpenBattleArena(initiator, target);
            }

            yield return new WaitForSeconds(1.0f);

            var skillConfig = initiator.GetSkillConfig(_skillType);
            //判断是攻击还是治疗
            //执行攻击或治疗
            //如果buff生效，添加buff到目标身上
            DebugManager.Instance.Log(skillConfig.AttrType);
            switch (skillConfig.AttrType)
            {
                case Enum.AttrType.PhysicalAttack:
                    initiator.Attack(target.GetPosition() + new Vector3(0, 0.6F, 0));
                    break;
                case Enum.AttrType.Cure:
                    initiator.Cure();
                    break;
            }
        }

        protected void HandleFightEvents(params object[] args)
        {
            if (null == args)
                return;
            if (args.Length <= 0)
                return;

            var strs = ((string)args[0]).Split('_');

            var eventOnwerUID = (int)args[1];
            if (0 == eventOnwerUID)
                return;

            var owner = RoleManager.Instance.GetRole(eventOnwerUID);
            string stateName = strs[0], secondStateName = strs[1];
            owner.HandleEvent(stateName, secondStateName);

            if ((stateName == "Attack" || stateName == "Cure") && secondStateName == "Take")
            {
                var target = RoleManager.Instance.GetRole(_targetID);
                var skillConfig = owner.GetSkillConfig(_skillType);
                //判断是攻击还是治疗
                //执行攻击或治疗
                //如果buff生效，添加buff到目标身上
                switch (skillConfig.AttrType)
                {
                    case Enum.AttrType.PhysicalAttack:
                        var attackPower = owner.GetAttackPower();
                        DebugManager.Instance.Log("AttackPower:" + attackPower);
                        target.Hit(attackPower);
                        target.AddBuffs(owner.GetAttackBuffs());
                        CameraMgr.Instance.ShakePosition();
                        break;
                    case Enum.AttrType.Cure:
                        var curePower = owner.GetCurePower();
                        target.Cured(curePower);
                        break;
                }
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_HP_Change, new object[] { _targetID });
            }
        }

        protected void Attack()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.Attacking);

            CoroutineMgr.Instance.StartCoroutine(PlayAttack());
        }

        private IEnumerator OnFinishAction(float waitingTime = 0f, bool isKill = false)
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

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Action_Over);
        }

        protected virtual void OnMoveEnd(params object[] args)
        {

        }

        protected virtual void StandByInitiator()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            _initiatorID = 0;
            _targetID = 0;
            initiator.SetState(Enum.RoleState.Over);

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Action_Over);
        }

        public virtual void OnTouch(GameObject obj)
        {

        }

        public virtual void OnClick(GameObject obj)
        {

        }
    }
}

