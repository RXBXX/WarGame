using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Skill
    {
        protected int _id;
        protected int _initiatorID;
        protected List<int> _targets = new List<int>();
        protected IEnumerator _coroutine;
        protected bool _isLockingCamera;
        protected List<MapObject> _arenaObjects = new List<MapObject>();

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

        public virtual void Dispose()
        {
            CloseBattleArena();
            RemoveListeners();
            CameraMgr.Instance.UnlockTarget();
        }

        public virtual void Play()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Battle);

            RoleManager.Instance.GetRole(_initiatorID).SetState(Enum.RoleState.Attacking);

            CoroutineMgr.Instance.StartCoroutine(DoPlay());
        }

        protected virtual IEnumerator DoPlay()
        {
            Prepare();

            if (!DatasMgr.Instance.GetSkipBattle())
            {
                yield return OpenBattleArena();
            }

            TriggerSkill();
        }

        protected virtual void Prepare()
        {
        }

        /// <summary>
        /// ´¥·¢¼¼ÄÜ
        /// </summary>
        protected virtual void TriggerSkill()
        { }

        protected virtual IEnumerator OpenBattleArena()
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHUDRoleVisible(false);
            }
            LockCamera();
            CameraMgr.Instance.OpenBattleArena();

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
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

            var skillName = GetConfig().GetTranslation("Name");
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, _targets, skillName});
            yield return new WaitForSeconds(1.0f);
        }

        protected virtual void CloseBattleArena()
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
                roles[i].SetHUDRoleVisible(true);
            }

            CameraMgr.Instance.CloseBattleArena();
            UnlockCamera();
        }

        protected virtual IEnumerator Over(float waitingTime = 0, bool isKill = false)
        {
            //DebugManager.Instance.Log("Over");
            yield return new WaitForSeconds(waitingTime);
            if (!DatasMgr.Instance.GetSkipBattle())
            {
                CloseBattleArena();
            }

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            _initiatorID = 0;
            _targets.Clear();

            initiator.SetState(Enum.RoleState.Over);

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Skill_Over);
        }

        public virtual void HandleFightEvents(int sender, string stateName, string secondStateName)
        {

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

        public virtual void ClickHexagon(int id)
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

        protected SkillConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _id);
        }

        public virtual void FocusIn(GameObject obj)
        { }
    }
}
