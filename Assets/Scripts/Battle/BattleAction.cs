using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class BattleAction
    {
        public int ID;
        protected int _initiatorID;
        protected int _targetID;
        protected int _skillID;
        protected List<string> _path; //Ó¢ÐÛÒÆ¶¯µÄÂ·¾¶
        protected bool _skipBattleShow = false;
        protected List<MapObject> _arenaObjects = new List<MapObject>();
        protected SkillAction _skillAction;
        protected bool _isLockingCamera;

        public BattleAction(int id)
        {
            this.ID = id;
            AddListeners();
        }

        public virtual void Dispose(bool save = false)
        {
            if (_initiatorID > 0)
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                if (initiator.GetState() > Enum.RoleState.Locked && initiator.GetState() < Enum.RoleState.Over)
                {
                    initiator.SetState(Enum.RoleState.Waiting, true);
                    switch (initiator.GetState())
                    {
                        case Enum.RoleState.Moving:
                        case Enum.RoleState.WaitingOrder:
                        case Enum.RoleState.WatingTarget:
                        case Enum.RoleState.Attacking:
                            initiator.UpdateHexagonID(_path[0]);
                            break;
                        case Enum.RoleState.ReturnMoving:
                            initiator.UpdateHexagonID(_path[_path.Count - 1]);
                            break;
                    }
                }
            }
            RemoveListeners();
        }

        protected virtual void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.Role_MoveEnd_Event, OnMoveEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Skill_Over, OnSkillOver);
        }

        protected virtual void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Role_MoveEnd_Event, OnMoveEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Skill_Over, OnSkillOver);
        }

        public void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (null != _skillAction)
                _skillAction.HandleFightEvents(sender, stateName, secondStateName);
        }

        protected virtual void OnSkillOver(params object[] args)
        {
            _skillAction.Dispose();
            _skillAction = null;

            OnActionOver();
        }

        protected virtual void OnMoveEnd(params object[] args)
        {

        }

        protected virtual void OnActionOver(params object[] args)
        {
            if (_initiatorID > 0)
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                _initiatorID = 0;
                _targetID = 0;
                initiator.SetState(Enum.RoleState.Over);
            }
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Action_Over, new object[] { ID });
        }

        protected virtual IEnumerator PlayActionOver(float delay)
        {
            yield return new WaitForSeconds(delay);
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            _initiatorID = 0;
            _targetID = 0;
            initiator.SetState(Enum.RoleState.Over);
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Action_Over, new object[] { ID });
        }

        public virtual void FocusIn(GameObject obj)
        {

        }

        public virtual void OnClickBegin(GameObject obg)
        {

        }

        public virtual void OnClickEnd()
        {

        }

        public virtual void OnClick(GameObject obj)
        {

        }

        public virtual void Update(float deltaTime)
        {
            if (null != _skillAction)
                _skillAction.Update(deltaTime);
        }

        protected void LockCamera()
        {
            _isLockingCamera = CameraMgr.Instance.Lock();
        }

        protected void UnlockCamera()
        {
            if (!_isLockingCamera)
                return;
            CameraMgr.Instance.Unlock();
            _isLockingCamera = false;
        }
    }
}

