using WarGame.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class HeroBattleAction : BattleAction
    {
        private int _touchRoleID;

        public HeroBattleAction() : base()
        {
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
            if (null == obj)
            {
                if (0 != _touchRoleID)
                {
                    var role = RoleManager.Instance.GetRole(_touchRoleID);
                    role.ResetHighLight();
                    _touchRoleID = 0;
                }
                return;
            }

            if (obj.tag == Enum.Tag.Hero.ToString() || obj.tag == Enum.Tag.Enemy.ToString())
            {
                var touchRoleID = obj.GetComponent<RoleBehaviour>().ID;
                if (_touchRoleID != touchRoleID)
                {
                    if (0 != _touchRoleID)
                    {
                        var role = RoleManager.Instance.GetRole(_touchRoleID);
                        role.ResetHighLight();
                    }

                    _touchRoleID = touchRoleID;
                    var touchRole = RoleManager.Instance.GetRole(_touchRoleID);
                    touchRole.HighLight();
                }
            }
            else if (0 != _touchRoleID)
            {
                var role = RoleManager.Instance.GetRole(_touchRoleID);
                role.ResetHighLight();
                _touchRoleID = 0;
            }

            if (obj.tag == Enum.Tag.Hexagon.ToString())
            {
                if (_initiatorID > 0)
                {
                    var role = RoleManager.Instance.GetRole(_initiatorID);
                    if (role.Type != Enum.RoleType.Hero)
                        return;

                    if (role.GetState() != Enum.RoleState.Waiting)
                        return;

                    var end = obj.GetComponent<HexagonBehaviour>().ID;
                    if (_touchingHexagon == end)
                        return;

                    _touchingHexagon = end;
                    var start = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
                    var hero = RoleManager.Instance.GetRole(_initiatorID);
                    MapManager.Instance.MarkingPath(start, end, hero.GetMoveDis());
                }
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
                var heroId = obj.GetComponent<RoleBehaviour>().ID;
                ClickHero(heroId);
            }
            else if (tag == Enum.Tag.Enemy.ToString())
            {

                var enemyId = obj.GetComponent<RoleBehaviour>().ID;
                ClickEnemy(enemyId);
            }
            else if (tag == Enum.Tag.Hexagon.ToString())
            {
                var hexagonID = obj.GetComponent<HexagonBehaviour>().ID;

                var heroID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
                if (heroID > 0)
                {
                    ClickHero(heroID);
                    return;
                }

                var enemyID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
                if (enemyID > 0)
                {
                    ClickEnemy(enemyID);
                    return;
                }

                ClickHexagon(obj.GetComponent<HexagonBehaviour>().ID);
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
        /// ��ʾ����ָ��
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
        /// ��ʾ����ָ��
        /// </summary>
        private void CloseInstruct()
        {
            HUDManager.Instance.RemoveHUD("HUDInstruct_Custom");
        }

        /// <summary>
        /// �ƶ�
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
        /// ���ȡ���ƶ��󣬸�λ
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

            StandByInitiator();

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
            _skillAction.Dispose();
            _skillAction = null;

            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.WaitingOrder);
        }
    }
}
