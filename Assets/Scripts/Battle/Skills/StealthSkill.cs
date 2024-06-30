using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class StealthSkill : Skill
    {
        public StealthSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Cure_End, OnCureEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Cure_End, OnCureEnd);
        }

        public override void Start()
        {
            Play();
        }

        public override void Play()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Battle);

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

            initiator.SetState(Enum.RoleState.Over);

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Skill_Over);
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            //var initiator = RoleManager.Instance.GetRole(sender);
            if ("Cure" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoStealth(_initiatorID);
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

        private void OnCureEnd(object[] args)
        {
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
            LockCamera();
            CameraMgr.Instance.OpenBattleArena();

            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + CameraMgr.Instance.GetMainCamForward() * 7;
            var pathCenter = initiator.GetPosition();

            var deltaVec = arenaCenter - pathCenter;

            var moveDuration = 0.2F;
            var hexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);
            hexagon.ChangeToArenaSpace(hexagon.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(hexagon);

            initiator.ChangeToArenaSpace(initiator.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(initiator);

            yield return new WaitForSeconds(moveDuration);

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HP, new object[] { new List<int> { _initiatorID }});
            yield return new WaitForSeconds(1.0f);
        }

        private void CloseBattleArena()
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
    }
}
