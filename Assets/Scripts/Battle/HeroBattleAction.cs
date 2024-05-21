using WarGame.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class HeroBattleAction : BattleAction
    {
        private int _touchingID = 0;

        public HeroBattleAction(LocatingArrow arrow) : base(arrow)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void AddListeners()
        {
            base.AddListeners();

            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Idle_Event, OnIdle);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Click_Skill, OnClickSkill);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Cancel_Event, OnCancel);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Cancel_Skill, OnCancelSkill);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Battle, Battle);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Idle_Event, OnIdle);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Click_Skill, OnClickSkill);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Cancel_Event, OnCancel);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Cancel_Skill, OnCancelSkill);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Battle, Battle);
            base.RemoveListeners();
        }


        public override void OnTouch(GameObject obj)
        {
            var touchingID = 0;
            string touchingHexagonID = null;
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
                else if (tag == Enum.Tag.Hexagon.ToString())
                {
                    var hexagonID = obj.GetComponent<HexagonBehaviour>().ID;

                    var roleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
                    if (roleID > 0)
                    {
                        touchingID = roleID;
                    }
                    else
                    {
                        var hexagon = MapManager.Instance.GetHexagon(hexagonID);
                        if (hexagon.IsReachable())
                        {
                            touchingHexagonID = hexagonID;
                        }
                    }
                }
            }

            if (0 != _touchingID && _touchingID != touchingID)
            {
                RoleManager.Instance.GetRole(_touchingID).ResetHighLight();
                _touchingID = 0;

                //EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Infor_Hide);
            }
            if (0 != touchingID && _touchingID != touchingID)
            {
                _touchingID = touchingID;
                var role = RoleManager.Instance.GetRole(_touchingID);
                role.HighLight();
                //EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Infor_Show, new object[] { _touchingID, role.HUDPoint.transform.position });
            }

            if (_initiatorID < 0 || null == touchingHexagonID || _touchingHexagon != touchingHexagonID)
            {
                MapManager.Instance.ClearMarkedPath();
            }
            if (_initiatorID > 0 && null != touchingHexagonID && _touchingHexagon != touchingHexagonID)
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                if (initiator.GetState() == Enum.RoleState.Waiting)
                    MapManager.Instance.MarkingPath(initiator.Hexagon, touchingHexagonID, initiator.GetMoveDis());
            }

            if (null == touchingHexagonID)
            {
                if (null != _arrow && _arrow.Active)
                {
                    _arrow.Active = false;
                }
                _touchingHexagon = null;
            }
            else if (touchingHexagonID != _touchingHexagon)
            {
                _touchingHexagon = touchingHexagonID;
                _arrow.Active = true;
                _arrow.Position = MapManager.Instance.GetHexagon(touchingHexagonID).GetPosition() + new Vector3(0, 0.24F, 0);
            }
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

            string hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(heroID);
            if (_initiatorID > 0)
            {
                if (_initiatorID == heroID)
                {
                    MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
                    hero.SetState(Enum.RoleState.WaitingOrder);
                    OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
                }
                else if (state != Enum.RoleState.Over)
                {
                    _initiatorID = heroID;
                    MapManager.Instance.MarkingRegion(hexagonID, hero.GetMoveDis(), hero.GetAttackDis(), Enum.RoleType.Hero);
                }
            }
            else if (state != Enum.RoleState.Over)
            {
                _initiatorID = heroID;
                MapManager.Instance.MarkingRegion(hexagonID, hero.GetMoveDis(), hero.GetAttackDis(), Enum.RoleType.Hero);
            }
        }

        private void ClickEnemy(int enemyId)
        {
            var enemy = RoleManager.Instance.GetRole(enemyId);
            if (enemy.GetState() != Enum.RoleState.Locked)
                return;

            if (null != _skillAction)
            {
                _skillAction.ClickEnemy(enemyId);
                return;
            }

            _initiatorID = 0;
            string hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(enemyId);
            MapManager.Instance.MarkingRegion(hexagonID, enemy.GetMoveDis(), enemy.GetAttackDis(), Enum.RoleType.Enemy);
        }

        private void ClickHexagon(string hexagonID)
        {
            if (_initiatorID <= 0)
                return;

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
        private void OpenInstruct(Enum.InstructType[] orders = null)
        {
            var role = RoleManager.Instance.GetRole(_initiatorID);
            HUDManager.Instance.AddHUD("HUD", "HUDInstruct", "HUDInstruct_Custom", role.HUDPoint, new object[] {
            role.GetConfig().CommonSkill,
            role.GetConfig().SpecialSkill,
            });
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        private void CloseInstruct()
        {
            HUDManager.Instance.RemoveHUD("HUDInstruct_Custom");
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void Move(string start, string end)
        {
            List<string> hexagons = MapManager.Instance.FindingPathForStr(start, end, Enum.RoleType.Hero, true);

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

            MapManager.Instance.ClearMarkedPath();
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
            _path = null;
        }

        private void Battle(params object[] args)
        {
            CloseInstruct();

            MapManager.Instance.ClearMarkedRegion();
        }

        private void OnIdle(params object[] args)
        {
            //DebugManager.Instance.Log("OnIdle");
            if (_initiatorID <= 0)
                return;

            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();

            OnActionOver();

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
            initiator.SetState(Enum.RoleState.WatingTarget);

            _skillAction = SkillFactory.Instance.GetSkill(_skillID, _initiatorID);
            _skillAction.Start();

            _arrow.SetLayer(8);
        }

        protected override void OnMoveEnd(params object[] args)
        {
            if (_initiatorID <= 0)
                return;

            var hero = RoleManager.Instance.GetRole(_initiatorID);
            var state = hero.GetState();
            if (state == Enum.RoleState.ReturnMoving || state == Enum.RoleState.WaitingOrder)
            {
                _initiatorID = 0;
                hero.SetState(Enum.RoleState.Waiting);
                return;
            }

            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
            hero.SetState(Enum.RoleState.WaitingOrder);
            OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
        }

        private void OnCancelSkill(object[] args)
        {
            _arrow.RecoverLayer();

            _skillAction.Dispose();
            _skillAction = null;

            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.WaitingOrder);
        }

        protected override void OnActionOver(params object[] args)
        {
            _arrow.RecoverLayer();
            base.OnActionOver(args);
        }
    }
}
