using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class CloneSkill : Skill
    {
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

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Cure" == stateName && "Take" == secondStateName)
            {
                _lock = 2;
                _targets.Add(BattleMgr.Instance.DoClone(_initiatorID, _targetHexagon.ID));
            }
        }

        protected override void Prepare()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
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

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnRoleCreate(object[] args)
        {
            if ((int)args[0] != _targets[0])
                return;

            var target = RoleManager.Instance.GetRole(_targets[0]);
            if (!_skipBattleShow)
            {
                target.SetHPVisible(false);
                target.ChangeToArenaSpace(_cloneTargetPos, 0);
                _arenaObjects.Add(target);
            }

            _lock--;
            if (_lock > 0)
                return;

            if (null != _coroutine)
                return;

            _coroutine = Over(1.5F);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }
    }
}
