using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class SkillAction
    {
        protected int _initiatorID;
        protected int _id;
        protected int _targetID;
        protected IEnumerator _coroutine;
        protected bool _skipBattleShow = false;

        public SkillAction(int id, int initiatorID)
        {
            this._id = id;
            this._initiatorID = initiatorID;

            AddListeners();

            CameraMgr.Instance.LockTarget();
        }

        protected virtual void AddListeners()
        {
        }

        protected virtual void RemoveListeners()
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Play()
        {
        }

        protected virtual IEnumerator Over(float waitingTime = 0, bool isKill = false)
        {
            yield return null;
        }

        public virtual void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
        }

        public virtual void Dispose()
        {
            RemoveListeners();
        }

        protected bool IsTarget(Enum.RoleType type)
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var skillConfig = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _id);
            if (skillConfig.TargetType == Enum.TargetType.Opponent)
                return type != initiator.Type;
            else if (skillConfig.TargetType == Enum.TargetType.Friend)
                return type == initiator.Type;
            return false;
        }

        public virtual void ClickHero(int id)
        {

        }

        public virtual void ClickEnemy(int id)
        {
        
        }

        public virtual void ClickHexagon(string id)
        {
        
        }

        public virtual void OnMoveEnd()
        { 

        }

        public virtual void Update(float deltaTime)
        { 
        
        }

        ///计算元素克制加成
        public float GetElementAdd(int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var add = 0.0f;
            var initiatorElement = initiator.GetElement();
            if (target.GetElementConfig().Restrain == initiatorElement)
            {
                add -= 0.1F;
            }

            var hexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);
            foreach (var v in MapManager.Instance.Dicections)
            {
                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(MapTool.Instance.GetHexagonKey(hexagon.coor + v));
                if (0 == roleID || roleID == _initiatorID)
                    continue;

                var role = RoleManager.Instance.GetRole(roleID);
                if (initiator.Type == role.Type && role.GetElementConfig().Reinforce == initiatorElement)
                {
                    add += 0.1F;
                }
            }

            return add;
        }
    }
}
