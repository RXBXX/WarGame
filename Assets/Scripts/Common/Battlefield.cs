using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public delegate void RoundFunc();

    public class BattleField
    {
        private int _initiatorID = 0, _targetID = 0; //��ǰѡ�е�Ӣ��
        private List<string> _path; //Ӣ���ƶ���·��
        private int _roundIndex = 0;
        private bool isHeroTurn = true;
        private string _touchingHexagon = null;

        public BattleField(string mapDir)
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Idle_Event, OnIdle);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Attack_Event, OnAttack);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Cancel_Event, OnCancel);
            EventDispatcher.Instance.AddListener(Enum.EventType.Role_MoveEnd_Event, OnMoveEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Attack_End, OnAttackEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Dead_End, OnDeadEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Attack, OnAIAttack);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_AI_Start, OnAIStart);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Cancel, OnCancelFight);

            MapManager.Instance.CreateMap(mapDir);

            var bornPoint = MapTool.Instance.GetHexagonKey(Vector3.zero);
            RoleManager.Instance.CreateHero(DatasMgr.Instance.GetRoleData(1), bornPoint);

            bornPoint = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(1, 0, 0));
            RoleManager.Instance.CreateHero(DatasMgr.Instance.GetRoleData(2), bornPoint);

            var bornPoint2 = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 3));
            RoleManager.Instance.CreateEnemy(new RoleData(11, 10003, 1, null, null), bornPoint2);

            bornPoint2 = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 4));
            RoleManager.Instance.CreateEnemy(new RoleData(12, 10004, 1, null, null), bornPoint2);

            UIManager.Instance.OpenPanel("Fight", "FightPanel");
        }

        public void Dispose()
        {
            CloseInstruct();

            RoleManager.Instance.Clear();
            MapManager.Instance.ClearMap();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Idle_Event, OnIdle);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Attack_Event, OnAttack);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Cancel_Event, OnCancel);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Role_MoveEnd_Event, OnMoveEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Attack_End, OnAttackEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Attacked_End, OnAttackedEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Dead_End, OnDeadEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Attack, OnAIAttack);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_AI_Start, OnAIStart);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Cancel, OnCancelFight);
        }

        public void Start()
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = roles.Count - 1; i >= 0; i--)
                roles[i].UpdateRound();
        }

        public void Touch(GameObject obj)
        {
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

        public void Click(GameObject obj)
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

        public void ClickHero(int heroID)
        {
            if (heroID <= 0)
                return;

            var hero = RoleManager.Instance.GetRole(heroID);
            var state = hero.GetState();
            if (state != Enum.RoleState.Waiting && state != Enum.RoleState.Over)
                return;

            string hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(heroID);
            MapManager.Instance.ClearMarkedRegion();

            if (_initiatorID > 0)
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                if (initiator.GetState() == Enum.RoleState.WatingTarget)
                {
                    var targetType = initiator.GetTargetType(Enum.SkillType.Common);
                    if (targetType != Enum.RoleType.Hero)
                        return;
                }

                if (initiator.GetState() == Enum.RoleState.WatingTarget)
                {
                    initiator.SetState(Enum.RoleState.Attacking);
                    Battle(heroID);
                }
                else if (_initiatorID == heroID)
                {
                    MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
                    hero.SetState(Enum.RoleState.WaitingOrder);
                    OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
                }
                else
                {
                    _initiatorID = heroID;
                    MapManager.Instance.MarkingRegion(hexagonID, hero.GetMoveDis(), hero.GetAttackDis(), Enum.RoleType.Hero);
                }
            }
            else
            {
                if (state == Enum.RoleState.Over)
                    return;
                _initiatorID = heroID;
                MapManager.Instance.MarkingRegion(hexagonID, hero.GetMoveDis(), hero.GetAttackDis(), Enum.RoleType.Hero);
            }
        }

        public void ClickEnemy(int enemyId)
        {
            if (enemyId <= 0)
                return;

            var enemy = RoleManager.Instance.GetRole(enemyId);
            if (enemy.GetState() != Enum.RoleState.Locked)
                return;

            if (_initiatorID > 0)
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                if (initiator.GetState() == Enum.RoleState.WatingTarget)
                {
                    var targetType = initiator.GetTargetType(Enum.SkillType.Common);
                    if (targetType != Enum.RoleType.Enemy)
                        return;
                }

                if (initiator.GetState() != Enum.RoleState.WatingTarget)
                {
                    _initiatorID = enemyId;
                    string hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(enemyId);
                    MapManager.Instance.ClearMarkedRegion();
                    MapManager.Instance.MarkingRegion(hexagonID, enemy.GetMoveDis(), enemy.GetAttackDis(), Enum.RoleType.Enemy);
                    return;
                }

                initiator.SetState(Enum.RoleState.Attacking);
                Battle(enemyId);
            }
            else
            {
                _initiatorID = enemyId;
                string hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(enemyId);
                MapManager.Instance.ClearMarkedRegion();
                MapManager.Instance.MarkingRegion(hexagonID, enemy.GetMoveDis(), enemy.GetAttackDis(), Enum.RoleType.Enemy);
            }
        }

        public void ClickHexagon(string hexagonID)
        {
            if (_initiatorID <= 0)
                return;

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
            HUDManager.Instance.AddHUD("HUD", "HUDInstruct", "HUDInstruct_Custom", role.HUDPoint);
        }

        /// <summary>
        /// ��ʾ����ָ��
        /// </summary>
        public void CloseInstruct()
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

            _touchingHexagon = null;
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

        private void Battle(int enemyID)
        {
            CloseInstruct();

            var initiatorID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var targetID = RoleManager.Instance.GetHexagonIDByRoleID(enemyID);

            List<string> hexagons = MapManager.Instance.FindingPathForStr(initiatorID, targetID, Enum.RoleType.Hero, false);
            if (null == hexagons || hexagons.Count <= 0)
                return;
            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }

            var hero = RoleManager.Instance.GetRole(_initiatorID);
            if (cost > hero.GetAttackDis())
                return;

            MapManager.Instance.ClearMarkedRegion();
            Attack(_initiatorID, enemyID);

            ExitGrayedMode();
        }

        private void OnIdle(params object[] args)
        {
            //DebugManager.Instance.Log("OnIdle");
            if (_initiatorID <= 0)
                return;

            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();

            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.Over);

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

        private void OnAttack(params object[] args)
        {
            EnterGrayedMode();
        }

        public void OnCheck(params object[] args)
        {
            UIManager.Instance.OpenPanel("Hero", "HeroPanel");
        }

        public void OnMoveEnd(params object[] args)
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

        public void OnAttackEnd(object[] args)
        {
            if (_targetID <= 0)
            {
                var initiator = RoleManager.Instance.GetRole(_initiatorID);
                initiator.SetState(Enum.RoleState.Over);
                _initiatorID = 0;
                _targetID = 0;

                OnFinishAction();
            }
        }

        private void OnAttackedEnd(object[] args)
        {
            //DebugManager.Instance.Log("OnAttackedEnd:" + args[0] + "_" +_targetID);
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            var target = RoleManager.Instance.GetRole(targetID);
            if (target.IsDead())
                return;

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.Over);
            _initiatorID = 0;
            _targetID = 0;

            OnFinishAction();
        }

        private void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.SetState(Enum.RoleState.Over);

            RoleManager.Instance.RemoveRole(targetID);

            _initiatorID = 0;
            _targetID = 0;

            OnFinishAction();
        }

        private void OnFinishAction()
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            if (heros.Count <= 0)
            {
                UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { false });
                return;
            }

            var enemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);
            if (enemys.Count <= 0)
            {
                UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { true });
                return;
            }

            if (isHeroTurn)
            {
                //�������Ӣ���Ƿ��ж�����
                for (int i = heros.Count - 1; i >= 0; i--)
                {
                    if (heros[i].GetState() != Enum.RoleState.Over)
                        return;
                }
            }

            if (isHeroTurn)
            {
                RoundFunc callback = () =>
                {
                    //���ҵ���һ��Ӧ���ж��ĵ���
                    for (int i = enemys.Count - 1; i >= 0; i--)
                    {
                        if (enemys[i].GetState() == Enum.RoleState.Locked)
                        {
                            enemys[i].SetState(Enum.RoleState.Waiting);
                            break;
                        }
                    }
                    isHeroTurn = false;
                };

                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundChange_Event, new object[] { callback });
            }

            if (!isHeroTurn)
            {
                //���ҵ���һ��Ӧ���ж��ĵ���
                for (int i = enemys.Count - 1; i >= 0; i--)
                {
                    if (enemys[i].GetState() == Enum.RoleState.Locked)
                    {
                        enemys[i].SetState(Enum.RoleState.Waiting);
                        return;
                    }
                }

                RoundFunc callback = () =>
                {
                    var roles = RoleManager.Instance.GetAllRoles();
                    for (int i = 0; i < roles.Count; i++)
                    {
                        roles[i].UpdateRound();
                    }
                    //for (int i = heros.Count - 1; i >= 0; i--)
                    //{
                    //    heros[i].UpdateRound();
                    //    heros[i].SetState(Enum.RoleState.Waiting);
                    //}
                    //for (int i = enemys.Count - 1; i >= 0; i--)
                    //{
                    //    enemys[i].UpdateRound();
                    //    enemys[i].SetState(Enum.RoleState.Locked);
                    //}
                    isHeroTurn = true;
                };
                _roundIndex += 1;
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundOver_Event, new object[] { _roundIndex, callback });
            }
        }

        public void Attack(int initiatorID, int targetID)
        {
            _initiatorID = initiatorID;
            _targetID = targetID;

            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(_targetID);
            var forward = target.GetPosition() - initiator.GetPosition();
            forward.y = 0;
            initiator.SetForward(forward);

            initiator.Attack();
        }

        public void OnAIAttack(object[] args)
        {
            Attack((int)args[0], (int)args[1]);
        }

        private void OnAIStart(object[] args)
        {
            _initiatorID = (int)args[0];
        }

        private void OnCancelFight(object[] args)
        {
            ExitGrayedMode();

            var role = RoleManager.Instance.GetRole(_initiatorID);
            role.SetState(Enum.RoleState.WaitingOrder);
        }

        private void HandleFightEvents(params object[] args)
        {
            if (null == args)
                return;
            if (args.Length <= 0)
                return;

            var strs = ((string)args[0]).Split('_');

            var initiatorID = (int)args[1];
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            string stateName = strs[0], secondStateName = strs[1];
            initiator.HandleEvent(stateName, secondStateName);

            if ((stateName == "Attack" || stateName == "Cure") && secondStateName == "Take")
            {
                var target = RoleManager.Instance.GetRole(_targetID);
                var skillConfig = initiator.GetSkillConfig(Enum.SkillType.Common);
                //�ж��ǹ�����������
                //ִ�й���������
                //���buff��Ч�����buff��Ŀ������
                switch (skillConfig.AttrType)
                {
                    case Enum.AttrType.Attack:
                        var attackPower = initiator.GetAttackPower();
                        target.Attacked(attackPower);
                        target.AddBuffs(initiator.GetAttackBuffs());

                        CameraMgr.Instance.ShakePosition();
                        break;
                    case Enum.AttrType.Cure:
                        var curePower = initiator.GetCurePower();
                        target.Cured(curePower);
                        break;
                }
            }
        }

        private void EnterGrayedMode()
        {
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiatorID);
            var hero = RoleManager.Instance.GetRole(_initiatorID);
            var region = MapManager.Instance.FindingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
            var regionDic = new Dictionary<string, bool>();
            foreach (var v in region)
                regionDic[v.id] = true;

            var targetType = hero.GetTargetType(Enum.SkillType.Common);
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].Type == targetType && roles[i].ID != _initiatorID && regionDic.ContainsKey(roles[i].hexagonID))
                    roles[i].SetLayer(8);
                else
                {
                    roles[i].SetColliderEnable(false);
                }
            }
            hero.SetState(Enum.RoleState.WatingTarget);

            CameraMgr.Instance.OpenGray();
        }

        private void ExitGrayedMode()
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].RecoverLayer();
                roles[i].SetColliderEnable(true);
            }

            CameraMgr.Instance.CloseGray();
        }
    }
}