using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace WarGame
{
    public class CloneSkill : Skill
    {
        private Hexagon _targetHexagon;
        private int _lock = 0;
        private Vector3 _targetDeltaVec;
        private Tweener _moveTweener;
        private Tweener _rotateTweener;

        public CloneSkill(int id, int initiatorID, int levelID) : base(id, initiatorID, levelID)
        {
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Cure_End, OnCureEnd);
            EventDispatcher.Instance.AddListener(Enum.Event.Role_Create_Success, OnRoleCreate);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Cure_End, OnCureEnd);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Role_Create_Success, OnRoleCreate);
        }

        public override void Start()
        {
            Play();
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Cure" == stateName && "Take" == secondStateName)
            {
                _lock = 2;
                var role = BattleMgr.Instance.DoClone(_initiatorID, _targetHexagon.ID, (int)GetConfig().Params[0]);
                role.Start();
                _targets.Add(role.ID);
            }
        }

        protected override void Prepare()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            //找出距离英雄最近的空位置，为后续分身做准备，不是寻路，不必考虑最短距离
            List<WGVector3> openDic = new List<WGVector3>();
            List<WGVector3> closeDic = new List<WGVector3>();
            openDic.Add(MapManager.Instance.GetHexagon(initiator.Hexagon).coor);
            while (null == _targetHexagon)
            {
                var count = openDic.Count;
                for (int i = 0; i < count; i++)
                {
                    var parentHexagon = openDic[i];
                    foreach (var v1 in MapManager.Instance.Dicections)
                    {
                        var hexCoor = parentHexagon + v1;

                        if (openDic.Contains(hexCoor) || closeDic.Contains(hexCoor))
                            continue;

                        openDic.Add(hexCoor);

                        var hexKey = MapTool.Instance.GetHexagonKey(parentHexagon + v1);
                        if (RoleManager.Instance.GetRoleIDByHexagonID(hexKey) > 0)
                            continue;

                        var hexagon = MapManager.Instance.GetHexagon(hexKey);

                        if (null == hexagon)
                            continue;

                        if (!hexagon.IsReachable())
                            continue;

                        var upHexagonKey = MapTool.Instance.GetHexagonKey(hexCoor + new WGVector3(0, 1, 0));
                        if (MapManager.Instance.ContainHexagon(upHexagonKey))
                            continue;

                        _targetHexagon = hexagon;
                    }
                }

                for (int i = count - 1; i >= 0; i--)
                {
                    closeDic.Add(openDic[i]);
                    openDic.RemoveAt(i);
                }
            }
        }

        protected override void TriggerSkill()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.Cure();
        }

        private void OnCureEnd(object[] args)
        {
            if ((int)args[0] != _initiatorID)
                return;

            _lock--;
            if (_lock > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over());
        }

        private void OnRoleCreate(object[] args)
        {
            //DebugManager.Instance.Log("OnRoleCreate");
            if ((int)args[0] != _targets[0])
                return;

            var target = RoleManager.Instance.GetRole(_targets[0]);
            if (!DatasMgr.Instance.GetSkipBattle())
            {
                target.SetHUDRoleVisible(false);
                target.ChangeToArenaSpace(target.GetPosition() + _targetDeltaVec, 0);
                _arenaObjects.Add(target);
            }

            AudioMgr.Instance.PlaySound("Assets/Audios/Clone.wav");
            var tran = target.GameObject.transform;
            tran.localScale = Vector3.zero;
            _moveTweener = tran.DOScale(target.GetScale(), 1.0f).SetEase(Ease.InOutQuart);
            _moveTweener.onComplete = () =>
            {
                _moveTweener = null;
                OnCloned();
            };

            _rotateTweener = tran.DOLocalRotate(new Vector3(0, 3600, 0), 1.0f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuart);
            _rotateTweener.onComplete = () => {
                _rotateTweener = null;
            };
        }

        protected override IEnumerator OpenBattleArena()
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
            _targetDeltaVec = deltaVec;

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
            _targetHexagon.ChangeToArenaSpace(_targetHexagon.GetPosition() + deltaVec, moveDuration);
            _arenaObjects.Add(_targetHexagon);

            yield return new WaitForSeconds(moveDuration);

            var skillName = GetConfig().GetTranslation("Name");
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HP, new object[] { new List<int> { _initiatorID }, _targets, skillName });
            yield return new WaitForSeconds(1.0f);
        }

        private void OnCloned()
        {
            _lock--;
            if (_lock > 0)
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over());
        }

        public override void Dispose()
        {
            if (null != _moveTweener)
            {
                _moveTweener.Kill();
                _moveTweener = null;
            }

            if (null != _rotateTweener)
            {
                _rotateTweener.Kill();
                _rotateTweener = null;
            }

            base.Dispose();
        }
    }
}
