using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public delegate void RoundFunc();

    public class BattleField
    {
        private int _levelID;
        private int _initiatorID = 0, _targetID = 0; //当前选中的英雄
        private List<string> _path; //英雄移动的路径
        private int _roundIndex = 0;
        private bool isHeroTurn = true;
        private string _touchingHexagon = null;
        private bool _skipBattleShow = false;
        private IEnumerator _coroutine;
        private List<MapObject> _arenaObjects = new List<MapObject>();

        public BattleField(int levelID)
        {
            _levelID = levelID;

            DatasMgr.Instance.StartLevel(levelID);

            var mapDir = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID).Map;
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

            var roleData = DatasMgr.Instance.GetRoleData(1);
            var levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, roleData.equipmentDic, roleData.skillDic);
            levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
            levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(Vector3.zero);
            RoleManager.Instance.CreateHero(levelRoleData);

            roleData = DatasMgr.Instance.GetRoleData(2);
            levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, roleData.equipmentDic, roleData.skillDic);
            levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
            levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(1, 0, 0));
            RoleManager.Instance.CreateHero(levelRoleData);

            levelRoleData = new LevelRoleData(11, 10003, 1, new Dictionary<Enum.EquipPlace, int>(), new Dictionary<int, int>());
            levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", 10004 * 1000 + 1).HP;
            levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 3));
            RoleManager.Instance.CreateEnemy(levelRoleData);

            levelRoleData = new LevelRoleData(12, 10004, 1, new Dictionary<Enum.EquipPlace, int>(), new Dictionary<int, int>());
            levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", 10004 * 1000 + 1).HP;
            levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(new Vector3(4, 0, 5));
            RoleManager.Instance.CreateEnemy(levelRoleData);

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
                    //MapManager.Instance.ClearMarkedRegion();

                    MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
                    hero.SetState(Enum.RoleState.WaitingOrder);
                    OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
                }
                else if (state != Enum.RoleState.Over)
                {
                    //MapManager.Instance.ClearMarkedRegion();
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
                    //MapManager.Instance.ClearMarkedRegion();
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
                //MapManager.Instance.ClearMarkedRegion();
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
        /// 显示操作指令
        /// </summary>
        private void OpenInstruct(Enum.InstructType[] orders = null)
        {
            var role = RoleManager.Instance.GetRole(_initiatorID);
            HUDManager.Instance.AddHUD("HUD", "HUDInstruct", "HUDInstruct_Custom", role.HUDPoint);
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        public void CloseInstruct()
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

            _touchingHexagon = null;
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
            ExitGrayedMode();
            Attack(_initiatorID, enemyID);
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
                _coroutine = OnFinishAction();
                CoroutineMgr.Instance.StartCoroutine(_coroutine);
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

            _coroutine = OnFinishAction();
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private void OnDeadEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targetID)
                return;

            _coroutine = OnFinishAction(true);
            CoroutineMgr.Instance.StartCoroutine(_coroutine);
        }

        private IEnumerator OnFinishAction(bool isKill = false)
        {
            yield return new WaitForSeconds(1.5f);
            if (0 != _targetID && !_skipBattleShow)
            {
                CloseBattleArena();
            }

            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            var target = RoleManager.Instance.GetRole(_targetID);

            initiator.SetState(Enum.RoleState.Over);
            if (isKill)
            {
                RoleManager.Instance.RemoveRole(_targetID);
            }
            _initiatorID = 0;
            _targetID = 0;


            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            if (heros.Count <= 0)
            {
                UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { false });
                CoroutineMgr.Instance.StopCoroutine(_coroutine);
                yield return null;
            }

            var enemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);
            if (enemys.Count <= 0)
            {
                DatasMgr.Instance.CompleteLevel(_levelID);
                UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { true });
                CoroutineMgr.Instance.StopCoroutine(_coroutine);
                yield return null;
            }

            if (isHeroTurn)
            {
                //检测所有英雄是否行动结束
                for (int i = heros.Count - 1; i >= 0; i--)
                {
                    if (heros[i].GetState() != Enum.RoleState.Over)
                    {
                        CoroutineMgr.Instance.StopCoroutine(_coroutine);
                        yield return null;
                    }
                }
            }

            if (isHeroTurn)
            {
                RoundFunc callback = () =>
                {
                    //查找到下一个应该行动的敌人
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
                //查找到下一个应该行动的敌人
                for (int i = enemys.Count - 1; i >= 0; i--)
                {
                    if (enemys[i].GetState() == Enum.RoleState.Locked)
                    {
                        enemys[i].SetState(Enum.RoleState.Waiting);
                        CoroutineMgr.Instance.StopCoroutine(_coroutine);
                        yield return null;
                    }
                }

                RoundFunc callback = () =>
                {
                    var roles = RoleManager.Instance.GetAllRoles();
                    for (int i = 0; i < roles.Count; i++)
                    {
                        roles[i].UpdateRound();
                    }
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
            var target = RoleManager.Instance.GetRole(targetID);

            CoroutineMgr.Instance.StartCoroutine(PlayAttack(initiator, target));
        }

        private IEnumerator PlayAttack(Role initiator, Role target)
        {
            var initiatorForward = target.GetPosition() - initiator.GetPosition();
            initiatorForward.y = 0;
            initiator.SetForward(initiatorForward);

            var targetForward = initiator.GetPosition() - target.GetPosition();
            targetForward.y = 0;
            target.SetForward(targetForward);

            if (!_skipBattleShow)
            {
                yield return OpenBattleArena(initiator, target);
            }

            yield return new WaitForSeconds(1.0f);
            initiator.Attack(target.GetPosition() + new Vector3(0, 0.6F, 0));
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
                //判断是攻击还是治疗
                //执行攻击或治疗
                //如果buff生效，添加buff到目标身上
                switch (skillConfig.AttrType)
                {
                    case Enum.AttrType.Attack:
                        var attackPower = initiator.GetAttackPower();
                        target.Hit(attackPower);
                        target.AddBuffs(initiator.GetAttackBuffs());
                        CameraMgr.Instance.ShakePosition();
                        break;
                    case Enum.AttrType.Cure:
                        var curePower = initiator.GetCurePower();
                        target.Cured(curePower);
                        break;
                }
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_HP_Change, new object[] { _targetID });
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

                if (roles[i].ID != _initiatorID && roles[i].ID != _targetID)
                    roles[i].SetHPVisible(false);
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

                if (roles[i].ID != _initiatorID && roles[i].ID != _targetID)
                    roles[i].SetHPVisible(true);
            }

            CameraMgr.Instance.CloseGray();
        }

        private IEnumerator OpenBattleArena(Role initiator, Role target)
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(false);
            }
            CameraMgr.Instance.OpenGray();

            var arenaCenter = CameraMgr.Instance.GetMainCamPosition() + CameraMgr.Instance.GetMainCamForward() * 5;
            var pathCenter = (target.GetPosition() + initiator.GetPosition()) / 2.0F;
            var deltaVec = arenaCenter - pathCenter;
            var path = MapManager.Instance.FindingPathForStr(initiator.hexagonID, target.hexagonID, Enum.RoleType.Hero, false);

            var moveDuration = 0.2F;
            for (int i = 0; i < path.Count; i++)
            {
                if (0 == i)
                {
                    initiator.ChangeToArenaSpace(initiator.GetPosition() + deltaVec, moveDuration);
                    initiator.SetLayer(8);
                    _arenaObjects.Add(initiator);
                }
                else if (path.Count - 1 == i)
                {
                    target.ChangeToArenaSpace(target.GetPosition() + deltaVec, moveDuration);
                    target.SetLayer(8);
                    _arenaObjects.Add(target);
                }
                var hexagon = MapManager.Instance.GetHexagon(path[i]);
                hexagon.ChangeToArenaSpace(hexagon.GetPosition() + deltaVec, moveDuration);
                hexagon.SetLayer(8);
                _arenaObjects.Add(hexagon);
                yield return new WaitForSeconds(moveDuration);
            }

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_HP, new object[] { _initiatorID, _targetID});
        }

        private void CloseBattleArena()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Close_HP);

            foreach (var v in _arenaObjects)
            {
                v.ChangeToMapSpace();
                v.RecoverLayer();
            }
            _arenaObjects.Clear();

            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].SetHPVisible(true);
            }

            CameraMgr.Instance.CloseGray();
        }
    }
}