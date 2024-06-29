using System.Collections;
using System.Collections.Generic;

namespace WarGame
{
    public class Skill
    {
        protected int _initiatorID;
        protected int _id;
        protected int _targetID;
        protected IEnumerator _coroutine;
        protected bool _skipBattleShow = false;
        protected bool _isLockingCamera;

        public Skill(int id, int initiatorID)
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
            CameraMgr.Instance.UnlockTarget();
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

        protected virtual void EnterGrayedMode()
        {
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var regionDic = MapManager.Instance.FindingAttackRegion(new List<string> { hexagonID }, initiator.GetAttackDis());

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

        protected SkillConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _id);
        }
    }
}
