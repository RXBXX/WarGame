using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class InspireSkillAction : SkillAction
    {
        private List<MapObject> _arenaObjects = new List<MapObject>();
        private List<int> _targets = new List<int>();

        public InspireSkillAction(int id, int initiatorID) : base(id, initiatorID)
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
            var roles = RoleManager.Instance.GetAllRoles();
            foreach (var v in roles)
            {
                if (v.ID == _initiatorID)
                    continue;

                if (IsTarget(v.Type))
                    _targets.Add(v.ID);
            }

            Play();
        }

        public override void Play()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Battle);

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.Attacking);

            CoroutineMgr.Instance.StartCoroutine(PlayAttack());
        }

        protected override IEnumerator Over(float waitingTime = 0, bool isKill = false)
        {
            yield return new WaitForSeconds(waitingTime);
            if (!_skipBattleShow)
            {
                CloseBattleArena();
            }

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            _initiatorID = 0;
            _targets.Clear();

            initiator.SetState(Enum.RoleState.Over);

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Skill_Over);
        }

        public override void Dispose()
        {
            RemoveListeners();
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            var initiator = RoleManager.Instance.GetRole(sender);
            if ("Cure" == stateName && "Take" == secondStateName)
            {
                initiator.ClearRage();
                foreach (var v in _targets)
                {
                    var target = RoleManager.Instance.GetRole(v);
                    var add = GetElementAdd(v);
                    target.Inspired(20 * (1 + add));
                }
            }
        }

        private IEnumerator PlayAttack()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            if (!_skipBattleShow)
            {
                yield return OpenBattleArena(initiator);
            }

            initiator.Cure();
        }

        private void OnCuredEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (!_targets.Contains(targetID))
                return;

            _targets.Remove(targetID);
            if (_targets.Count > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private IEnumerator OpenBattleArena(Role initiator)
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(false);
            }
            CameraMgr.Instance.Lock();
            CameraMgr.Instance.OpenGray();

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

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, _targets});
            yield return new WaitForSeconds(1.0f);
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
    }
}
