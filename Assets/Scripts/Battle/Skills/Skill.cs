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
        protected bool _isLockingCamera;
        protected List<MapObject> _arenaObjects = new List<MapObject>();
        private int _touchingID;
        private bool _isPlaing = false;
        private Coroutine _playCoroutine;
        protected Coroutine _attackCoroutine;
        protected int _levelID;

        public Skill(int id, int initiatorID, int levelID)
        {
            this._id = id;
            this._initiatorID = initiatorID;
            _levelID = levelID;

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
            if (null != _playCoroutine)
            {
                CoroutineMgr.Instance.StopCoroutine(_playCoroutine);
                _playCoroutine = null;
            }

            if (null != _attackCoroutine)
            {
                CoroutineMgr.Instance.StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }

            if (!DatasMgr.Instance.GetSkipBattle())
            {
                CloseBattleArena();
            }

            RemoveListeners();
            CameraMgr.Instance.UnlockTarget();
        }

        public virtual void Play()
        {
            _isPlaing = true;
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Battle);

            RoleManager.Instance.GetRole(_initiatorID).SetState(Enum.RoleState.Attacking);

            _playCoroutine = CoroutineMgr.Instance.StartCoroutine(DoPlay());
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
        /// 触发技能
        /// </summary>
        protected virtual void TriggerSkill()
        { }

        protected Vector3 GetArenaDeltaVec()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + CameraMgr.Instance.GetMainCamForward() * 7;
            var pathCenter = initiator.GetPosition();
            foreach (var v in _targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                pathCenter += target.GetPosition();
            }
            pathCenter /= _targets.Count + 1;

            return arenaCenter - pathCenter;
        }

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

            var deltaVec = GetArenaDeltaVec();

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
            DebugManager.Instance.Log("OpenBattleArena");
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, _targets, skillName });
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
        {
            if (_isPlaing)
                return;

            var touchingID = 0;
            if (null != obj)
            {
                var tag = obj.tag;
                if (tag == Enum.Tag.Hero.ToString())
                {
                    touchingID = obj.GetComponent<RoleBehaviour>().ID;
                }
                else if (tag == Enum.Tag.Enemy.ToString())
                {
                    touchingID = obj.GetComponent<RoleBehaviour>().ID;
                }
            }

            if (touchingID != _touchingID)
            {
                if (0 != _touchingID)
                {
                    DebugManager.Instance.Log("CancelPreview");
                    CancelPreview();
                    _touchingID = 0;
                }
                if (0 != touchingID)
                {
                    DebugManager.Instance.Log("Preview");
                    _touchingID = touchingID;
                    Preview(_touchingID);
                }
            }
        }

        /// <summary>
        /// 技能效果预览
        /// </summary>
        /// <param name="id"></param>
        protected virtual void Preview(int id)
        { }

        /// <summary>
        /// 技能效果取消预览
        /// </summary>
        protected virtual void CancelPreview()
        { }
    }
}
