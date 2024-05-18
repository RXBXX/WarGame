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
        protected int _skillID;
        protected List<string> _path; //Ó¢ÐÛÒÆ¶¯µÄÂ·¾¶
        protected bool _skipBattleShow = false;
        protected List<MapObject> _arenaObjects = new List<MapObject>();
        protected string _touchingHexagon = null;
        protected SkillAction _skillAction;

        public BattleAction()
        {
            AddListeners();
        }

        public virtual void Dispose()
        {
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

        private void OnSkillOver(params object[] args)
        {
            _skillAction.Dispose();
            _skillAction = null;

            OnActionOver();
            //EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Action_Over);
        }

        protected virtual void OnMoveEnd(params object[] args)
        {

        }

        protected virtual void OnActionOver(params object[] args)
        {
            DebugManager.Instance.Log("OnActionOver:" + _initiatorID);
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

