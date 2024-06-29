using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class CloneSkill : Skill
    {
        protected List<MapObject> _arenaObjects = new List<MapObject>();
        private Hexagon _targetHexagon;
        private Vector3 _cloneTargetPos;
        private int _lock = 0;

        public CloneSkill(int id, int initiatorID) : base(id, initiatorID)
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

        public override void Dispose()
        {
            ExitGrayedMode();
            base.Dispose();
        }

        public override void Play()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Battle);

            ExitGrayedMode();

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.Attacking);

            CoroutineMgr.Instance.StartCoroutine(PlayAttack());
        }

        protected override IEnumerator Over(float waitingTime = 0, bool isKill = false)
        {
            yield return new WaitForSeconds(waitingTime);
            if (0 != _targetID && !_skipBattleShow)
            {
                CloseBattleArena();
            }

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            _initiatorID = 0;
            _targetID = 0;
            initiator.SetState(Enum.RoleState.Over);

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Skill_Over);
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Cure" == stateName && "Take" == secondStateName)
            {
                _lock = 2;
                _targetID = BattleMgr.Instance.DoClone(_initiatorID, _targetHexagon.ID);
            }
        }

        private IEnumerator PlayAttack()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var initiatorHexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);

            //找出距离英雄最近的空位置，为后续分身做准备，不是寻路，不必考虑最短距离
            List<Vector3> openDic = new List<Vector3>();
            List<Vector3> closeDic = new List<Vector3>();
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
                        if (hexagon.IsReachable())
                        {
                            _targetHexagon = hexagon;
                            break;
                        }
                    }
                }

                for (int i = count - 1; i >= 0; i--)
                {
                    closeDic.Add(openDic[i]);
                    openDic.RemoveAt(i);
                }
            }

            if (!_skipBattleShow)
            {
                yield return OpenBattleArena(initiator, _targetHexagon);
            }

            //yield return new WaitForSeconds(1.0f);
            initiator.Cure();
        }

        private void OnCureEnd(object[] args)
        {
            _lock--;
            if (_lock > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnRoleCreate(object[] args)
        {
            if ((int)args[0] != _targetID)
                return;

            var target = RoleManager.Instance.GetRole(_targetID);
            if (!_skipBattleShow)
            {
                target.SetHPVisible(false);
                target.ChangeToArenaSpace(_cloneTargetPos, 0);
                _arenaObjects.Add(target);
            }
            //var initiator = RoleManager.Instance.GetRole(_initiatorID);
            //var targetTra = target.GameObject.transform;
            //targetTra.position = initiator.GameObject.transform.position;
            //targetTra.localScale = Vector3.zero;
            //targetTra.DOMove( _cloneTargetPos, 0.5F);
            //targetTra.DOScale(1, 0.5F);

            _lock--;
            if (_lock > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        protected virtual IEnumerator OpenBattleArena(Role initiator, Hexagon targetHexagon)
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(false);
            }

            LockCamera();
            CameraMgr.Instance.OpenBattleArena();
            var moveDuration = 0.2F;

            var camForward = CameraMgr.Instance.GetMainCamForward();
            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + camForward * 10;
            var initiatorToTargetDis = Vector3.Distance(targetHexagon.GetPosition(), initiator.GetPosition());
            var rightDir = CameraMgr.Instance.GetMainCamRight();
            var initiatorPos = arenaCenter - rightDir * initiatorToTargetDis / 2;
            var targetPos = arenaCenter + rightDir * initiatorToTargetDis / 2;
            var hexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);
            hexagon.SetForward(camForward - new Vector3(0, camForward.y, 0));
            hexagon.ChangeToArenaSpace(arenaCenter - rightDir * initiatorToTargetDis / 2 - CommonParams.Offset, moveDuration);
            _arenaObjects.Add(hexagon);

            initiator.SetForward(targetPos - initiatorPos);
            initiator.ChangeToArenaSpace(initiatorPos, moveDuration);
            _arenaObjects.Add(initiator);

            yield return new WaitForSeconds(moveDuration);

            _cloneTargetPos = arenaCenter + rightDir * initiatorToTargetDis / 2;
            targetHexagon.SetForward(camForward - new Vector3(0, camForward.y, 0));
            targetHexagon.ChangeToArenaSpace(arenaCenter + rightDir * initiatorToTargetDis / 2 - CommonParams.Offset, moveDuration);
            _arenaObjects.Add(targetHexagon);

            yield return new WaitForSeconds(moveDuration);

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HP, new object[] { new List<int> { _initiatorID }});
            yield return new WaitForSeconds(1);
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
                roles[i].SetHPVisible(true);
            }

            CameraMgr.Instance.CloseBattleArena();
            UnlockCamera();
        }
    }
}
