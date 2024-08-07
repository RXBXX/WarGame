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
        protected List<int> _path; //Ó¢ÐÛÒÆ¶¯µÄÂ·¾¶
        protected List<MapObject> _arenaObjects = new List<MapObject>();
        protected Skill _skillAction;
        protected bool _isLockingCamera;
        protected int _levelID;

        public BattleAction(int id, int levelID)
        {
            this.ID = id;
            _levelID = levelID;
            AddListeners();
        }

        public virtual void Dispose(bool save = false)
        {
            RemoveListeners();

            if (null != _skillAction)
            {
                _skillAction.Dispose();
                _skillAction = null;
            }
        }

        protected virtual void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Role_MoveEnd_Event, OnMoveEnd);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Skill_Over, OnSkillOver);
        }

        protected virtual void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Role_MoveEnd_Event, OnMoveEnd);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Skill_Over, OnSkillOver);
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
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Action_Over, new object[] { ID });
        }

        //protected virtual IEnumerator PlayActionOver(float delay)
        //{
        //    yield return new WaitForSeconds(delay);
        //    var initiator = RoleManager.Instance.GetRole(_initiatorID);
        //    _initiatorID = 0;
        //    _targetID = 0;
        //    initiator.SetState(Enum.RoleState.Over);
        //    EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Action_Over, new object[] { ID });
        //}

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

