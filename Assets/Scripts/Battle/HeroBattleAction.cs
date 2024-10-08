using WarGame.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class HeroBattleAction : BattleAction
    {
        protected int _touchingHexagon = -1;

        public HeroBattleAction(int id, int levelID) : base(id, levelID)
        {
            Type = Enum.ActionType.HeroAction;
        }

        public override void Dispose(bool save = false)
        {
            if (_initiatorID > 0)
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                switch (initiator.GetState())
                {
                    case Enum.RoleState.Locked:
                        break;
                    case Enum.RoleState.Waiting:
                        break;
                    case Enum.RoleState.Moving:
                    case Enum.RoleState.WaitingOrder:
                    case Enum.RoleState.WatingTarget:
                    case Enum.RoleState.Attacking:
                        initiator.SetState(Enum.RoleState.Waiting, true);
                        initiator.UpdateHexagonID(_path[0]);
                        break;
                    case Enum.RoleState.ReturnMoving:
                        initiator.SetState(Enum.RoleState.Waiting, true);
                        initiator.UpdateHexagonID(_path[_path.Count - 1]);
                        break;
                    case Enum.RoleState.Over:
                        break;
                }
            }

            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();
            base.Dispose(save);
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            EventDispatcher.Instance.AddListener(Enum.Event.HUDInstruct_Idle_Event, OnIdle);
            EventDispatcher.Instance.AddListener(Enum.Event.HUDInstruct_Click_Skill, OnClickSkill);
            EventDispatcher.Instance.AddListener(Enum.Event.HUDInstruct_Attack, OnClickAttack);
            EventDispatcher.Instance.AddListener(Enum.Event.HUDInstruct_Cancel_Event, OnCancel);
            EventDispatcher.Instance.AddListener(Enum.Event.HUDInstruct_Cancel_Skill, OnCancelSkill);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Battle, Battle);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.HUDInstruct_Idle_Event, OnIdle);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HUDInstruct_Click_Skill, OnClickSkill);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HUDInstruct_Cancel_Event, OnCancel);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HUDInstruct_Cancel_Skill, OnCancelSkill);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Battle, Battle);
            base.RemoveListeners();
        }


        public override void FocusIn(GameObject obj)
        {
            int touchingHexagonID = -1;
            if (null != obj)
            {
                var tag = obj.tag;
                if (tag == Enum.Tag.Hexagon.ToString())
                {
                    var hexagonID = obj.GetComponent<HexagonBehaviour>().ID;

                    var roleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
                    if (roleID <= 0)
                    {
                        var hexagon = MapManager.Instance.GetHexagon(hexagonID);
                        if (hexagon.IsReachable())
                        {
                            touchingHexagonID = hexagonID;
                        }
                    }
                }
            }

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            if (null != initiator && -1 != touchingHexagonID && _touchingHexagon != touchingHexagonID && initiator.GetState() == Enum.RoleState.Waiting)
            {
                _touchingHexagon = touchingHexagonID;
                MapManager.Instance.MarkingPath(initiator.Hexagon, touchingHexagonID, initiator.GetMoveDis());
            }
            else if (-1 != _touchingHexagon && _touchingHexagon != touchingHexagonID)
            {
                _touchingHexagon = -1;
                MapManager.Instance.ClearMarkedPath();
            }

            if (null != _skillAction)
                _skillAction.FocusIn(obj);
        }

        public override void OnClick(GameObject obj)
        {
            if (_initiatorID > 0)
            {
                var role = RoleManager.Instance.GetRole(_initiatorID);
                var state = role.GetState();
                if (state == Enum.RoleState.Moving || state == Enum.RoleState.ReturnMoving || state == Enum.RoleState.WaitingOrder || state == Enum.RoleState.Attacking)
                    return;
            }

            var tag = obj.tag;
            if (tag == Enum.Tag.Hero.ToString())
            {
                ClickHero(obj.GetComponent<RoleBehaviour>().ID);
            }
            else if (tag == Enum.Tag.Enemy.ToString())
            {
                ClickEnemy(obj.GetComponent<RoleBehaviour>().ID);
            }
            else if (tag == Enum.Tag.Hexagon.ToString())
            {
                var hexagonID = obj.GetComponent<HexagonBehaviour>().ID;

                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
                if (roleID > 0)
                {
                    var role = RoleManager.Instance.GetRole(roleID);
                    if (role.Type == Enum.RoleType.Hero)
                        ClickHero(roleID);
                    else
                        ClickEnemy(roleID);
                    return;
                }

                ClickHexagon(hexagonID);
            }
        }

        private void ClickHero(int heroID)
        {
            var hero = RoleManager.Instance.GetRole(heroID);
            var state = hero.GetState();
            if (state != Enum.RoleState.Waiting && state != Enum.RoleState.Over)
                return;

            if (null != _skillAction)
            {
                _skillAction.ClickHero(heroID);
                return;
            }

            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(heroID);
            if (_initiatorID > 0 && _initiatorID == heroID)
            {
                MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
                hero.SetState(Enum.RoleState.WaitingOrder);
                OpenInstruct();
            }
            else if (hero.CanAction())
            {
                _initiatorID = heroID;
                MapManager.Instance.MarkingRegion(hexagonID, hero.GetMoveDis(), hero.GetAttackDis(), Enum.RoleType.Hero);
            }
        }

        private void ClickEnemy(int enemyId)
        {
            var enemy = RoleManager.Instance.GetRole(enemyId);
            DebugManager.Instance.Log(enemy.GetState());
            if (enemy.GetState() != Enum.RoleState.Locked && enemy.GetState() != Enum.RoleState.Over)
                return;

            if (null != _skillAction)
            {
                _skillAction.ClickEnemy(enemyId);
                return;
            }

            _initiatorID = 0;
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(enemyId);
            MapManager.Instance.MarkingRegion(hexagonID, enemy.GetMoveDis(), enemy.GetAttackDis(), Enum.RoleType.Enemy);
        }

        private void ClickHexagon(int hexagonID)
        {
            if (_initiatorID <= 0)
                return;

            MapManager.Instance.ClearMarkedPath();

            if (null != _skillAction)
            {
                _skillAction.ClickHexagon(hexagonID);
                return;
            }

            var role = RoleManager.Instance.GetRole(_initiatorID);
            if (role.GetState() != Enum.RoleState.Waiting)
                return;

            Move(RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID), hexagonID);
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        private void OpenInstruct()
        {
            var role = RoleManager.Instance.GetRole(_initiatorID);
            var specialSkill = role.GetConfig().SpecialSkill;
            //HUDManager.Instance.AddHUD<HUDInstruct>("HUD", "HUDInstruct", "HUDInstruct_Custom", role.HUDPoint, new object[] {
            //role.GetConfig().CommonSkill,
            //specialSkill,
            //BattleMgr.Instance.IsReadySpecialSkill(_initiatorID, (Enum.Skill)specialSkill)
            //});
            var rageFilled = BattleMgr.Instance.IsReadySpecialSkill(_initiatorID, (Enum.Skill)specialSkill);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_OpenInstruct, new object[] { role.HUDPoint, role.GetConfig().CommonSkill, specialSkill, rageFilled });
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        private void CloseInstruct()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_CloseInstruct);
            //HUDManager.Instance.RemoveHUD("HUDInstruct_Custom");
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void Move(int start, int end)
        {
            List<int> hexagons = MapManager.Instance.FindingPathForStr(start, end, RoleManager.Instance.GetRole(_initiatorID).GetMoveDis(), Enum.RoleType.Hero);

            if (null == hexagons || hexagons.Count <= 0)
                return;

            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }
            var hero = RoleManager.Instance.GetRole(_initiatorID);
            if (cost > hero.GetMoveDis())
                return;

            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.Moving);
            _path = hexagons;

            MapManager.Instance.ClearMarkedRegion();

            hero.Move(hexagons);
        }

        /// <summary>
        /// 玩家取消移动后，复位
        /// </summary>
        private void ReverseMove()
        {
            if (null == _path)
                return;

            _path.Reverse();
            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.Move(_path);
            //_path = null;
        }

        private void Battle(params object[] args)
        {
            CloseInstruct();

            MapManager.Instance.ClearMarkedRegion();
        }

        private void OnIdle(params object[] args)
        {
            if (_initiatorID <= 0)
                return;

            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();

            OnActionOver(new object[] { 0 });

            _path = null;
        }

        private void OnCancel(params object[] args)
        {
            if (_initiatorID <= 0)
                return;

            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();

            var role = RoleManager.Instance.GetRole(_initiatorID);
            if (null != _path && _path.Count > 0)
            {
                role.SetState(Enum.RoleState.ReturnMoving);
                ReverseMove();
            }
            else
            {
                OnMoveEnd();
            }
        }

        private void OnClickSkill(params object[] args)
        {
            _skillID = (int)args[0];

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            //DebugManager.Instance.Log("Initiator:" + ID +"_"+_initiatorID);
            initiator.SetState(Enum.RoleState.WatingTarget); //这里会有initiator为null的报错

            CameraMgr.Instance.SetTarget(_initiatorID);

            MapManager.Instance.ClearMarkedRegion();
            _skillAction = Factory.Instance.GetSkill(_skillID, _initiatorID, _levelID);
            _skillAction.Start();
        }

        protected override void OnMoveEnd(params object[] args)
        {
            if (null != _skillAction)
            {
                _skillAction.OnMoveEnd();
                return;
            }

            if (_initiatorID <= 0)
                return;

            var hero = RoleManager.Instance.GetRole(_initiatorID);
            var state = hero.GetState();
            if (state == Enum.RoleState.ReturnMoving || state == Enum.RoleState.WaitingOrder)
            {
                if (state == Enum.RoleState.ReturnMoving)
                    _path = null;
                _initiatorID = 0;
                hero.SetState(Enum.RoleState.Waiting);
                return;
            }

            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
            hero.SetState(Enum.RoleState.WaitingOrder);
            OpenInstruct();
        }

        private void OnCancelSkill(object[] args)
        {
            _skillAction.Dispose();
            _skillAction = null;

            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.WaitingOrder);
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            MapManager.Instance.MarkingRegion(hexagonID, 0, role.GetAttackDis(), Enum.RoleType.Hero);
        }

        private void OnClickAttack(object[] args)
        {
        }

        public override bool CanSkip()
        {
            if (0 == _initiatorID)
                return true;
            else
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                if (initiator.GetState() == Enum.RoleState.Waiting)
                    return true;
            }
            return false;
        }
    }
}
