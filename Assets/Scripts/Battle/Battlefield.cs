using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public delegate void RoundFunc();

    public class BattleField
    {
        private bool _isStart = false;
        private int _levelID;
        private int _roundIndex = 0;
        private bool isHeroTurn = true;
        private BattleAction _action;
        private LocatingArrow _arrow;
        private Coroutine _coroutine;

        public BattleField(int levelID)
        {
            UIManager.Instance.OpenPanel("Load", "LoadPanel");

            _levelID = levelID;

            DatasMgr.Instance.StartLevel(levelID);

            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Action_Over, OnFinishAction);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Skip_Rount, OnSkipRound);

            var mapDir = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID).Map;
            LevelMapPlugin levelPlugin = Tool.Instance.ReadJson<LevelMapPlugin>(mapDir);

            MapManager.Instance.CreateMap(levelPlugin.hexagons);

            var levelData = DatasMgr.Instance.GetLevelData(levelID);
            if (levelData.heros.Count <= 0)
            {
                var roleData = DatasMgr.Instance.GetRoleData(1);
                var equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                foreach (var v in roleData.equipmentDic)
                {
                    equipDataDic.Add(v.Key, DatasMgr.Instance.GetEquipmentData(v.Value));
                }
                var levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, Enum.RoleState.Waiting, equipDataDic, roleData.talentDic);
                levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
                levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(Vector3.zero);
                RoleManager.Instance.CreateHero(levelRoleData);
                levelData.heros.Add(levelRoleData);

                roleData = DatasMgr.Instance.GetRoleData(2);
                equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                foreach (var v in roleData.equipmentDic)
                {
                    equipDataDic.Add(v.Key, DatasMgr.Instance.GetEquipmentData(v.Value));
                }
                levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, Enum.RoleState.Waiting, equipDataDic, roleData.talentDic);
                levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
                levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(1, 0, 0));
                RoleManager.Instance.CreateHero(levelRoleData);
                levelData.heros.Add(levelRoleData);

                roleData = DatasMgr.Instance.GetRoleData(3);
                equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                foreach (var v in roleData.equipmentDic)
                {
                    equipDataDic.Add(v.Key, DatasMgr.Instance.GetEquipmentData(v.Value));
                }
                levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, Enum.RoleState.Waiting, equipDataDic, roleData.talentDic);
                levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
                levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(1, 0, 1));
                RoleManager.Instance.CreateHero(levelRoleData);
                levelData.heros.Add(levelRoleData);

                roleData = DatasMgr.Instance.GetRoleData(4);
                equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                foreach (var v in roleData.equipmentDic)
                {
                    equipDataDic.Add(v.Key, DatasMgr.Instance.GetEquipmentData(v.Value));
                }
                levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, Enum.RoleState.Waiting, equipDataDic, roleData.talentDic);
                levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
                levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(2, 0, 1));
                RoleManager.Instance.CreateHero(levelRoleData);
                levelData.heros.Add(levelRoleData);
            }
            else
            {
                foreach (var v in levelData.heros)
                {
                    RoleManager.Instance.CreateHero(v);
                }
            }

            if (levelData.enemys.Count <= 0)
            {
                for (int i = 0; i < levelPlugin.enemys.Length; i++)
                {
                    var enemyConfig = ConfigMgr.Instance.GetConfig<LevelEnemyConfig>("LevelEnemyConfig", levelPlugin.enemys[i].configId);
                    var equipDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                    for (int j = 0; j < enemyConfig.Equips.Length; j++)
                    {
                        var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", enemyConfig.Equips[j]);
                        var equipTypeConfig = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (int)equipConfig.Type);
                        equipDic[equipTypeConfig.Place] = new EquipmentData(0, equipConfig.ID);
                    }
                    var levelRoleData = new LevelRoleData(enemyConfig.ID, enemyConfig.RoleID, enemyConfig.Level, Enum.RoleState.Locked, equipDic, null);
                    levelRoleData.hp = enemyConfig.HP;
                    levelRoleData.hexagonID = levelPlugin.enemys[i].hexagonID;
                    RoleManager.Instance.CreateEnemy(levelRoleData);
                    levelData.enemys.Add(levelRoleData);
                }
            }
            else
            {
                foreach (var v in levelData.enemys)
                {
                    RoleManager.Instance.CreateEnemy(v);
                }
            }

            _arrow = new LocatingArrow();
        }

        public void Update(float deltaTime)
        {
            if (_isStart)
                return;

            var progress = (RoleManager.Instance.GetLoadingProgress() + MapManager.Instance.GetLoadingProgress() + _arrow.GetLoadingProgress()) / 3;
            EventDispatcher.Instance.PostEvent(Enum.EventType.Scene_Load_Progress, new object[] { progress });

            if (progress >= 1 && null == _coroutine)
            {
                //延迟一帧开始，防止场景内对象没有初始化完成
                _coroutine = CoroutineMgr.Instance.StartCoroutine(Start());
            }
        }

        public void Dispose()
        {
            CameraMgr.Instance.SetTarget(0);

            if (null != _action)
            {
                _action.Dispose();
                _action = null;
            }

            RoleManager.Instance.Clear();
            MapManager.Instance.ClearMap();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Action_Over, OnFinishAction);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Skip_Rount, OnSkipRound);
        }

        private IEnumerator Start()
        {
            yield return null;

            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            CameraMgr.Instance.SetTarget(heros[0].ID);

            UIManager.Instance.ClosePanel("LoadPanel");
            _isStart = true;
            UIManager.Instance.OpenPanel("Fight", "FightPanel");
            //var roles = RoleManager.Instance.GetAllRoles();
            //for (int i = roles.Count - 1; i >= 0; i--)
            //    roles[i].UpdateRound();

            _action = new HeroBattleAction(_arrow);
        }

        public IEnumerator DelayStart()
        {
            yield return new WaitForSeconds(5);
            Start();
        }

        public void Touch(GameObject obj)
        {
            if (!_isStart)
                return;

            if (null == _action)
                return;

            _action.OnTouch(obj);
        }

        public void Click(GameObject obj)
        {
            if (!_isStart)
                return;

            if (null == _action)
                return;
            _action.OnClick(obj);
        }

        private void DisposeAction()
        {
            //DebugManager.Instance.Log("DisposeAction");
            _action.Dispose();
            _action = null;
        }

        private void OnFinishAction(params object[] args)
        {
            DisposeAction();

            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            if (heros.Count <= 0)
            {
                UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { false });
                return;
            }

            var enemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);
            if (enemys.Count <= 0)
            {
                DatasMgr.Instance.CompleteLevel(_levelID);
                UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { true });
                return;
            }

            if (isHeroTurn)
            {
                //检测所有英雄是否行动结束
                for (int i = heros.Count - 1; i >= 0; i--)
                {
                    if (heros[i].GetState() != Enum.RoleState.Over)
                    {
                        _action = new HeroBattleAction(_arrow);
                        return;
                    }
                }
            }

            if (isHeroTurn)
            {
                RoundFunc callback = () =>
                {
                    ////将所有英雄移除置灰状态
                    //for (int i = heros.Count - 1; i >= 0; i--)
                    //{
                    //    heros[i].SetGrayed(false);
                    //}

                    //查找到下一个应该行动的敌人
                    for (int i = enemys.Count - 1; i >= 0; i--)
                    {
                        if (enemys[i].GetState() == Enum.RoleState.Locked)
                        {
                            _action = new EnemyBattleAction();
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
                        _action = new EnemyBattleAction();
                        enemys[i].SetState(Enum.RoleState.Waiting);
                        return;
                    }
                }

                RoundFunc callback = () =>
                {
                    ////将所有敌人移除置灰状态
                    //for (int i = enemys.Count - 1; i >= 0; i--)
                    //{
                    //    enemys[i].SetGrayed(false);
                    //}

                    var roles = RoleManager.Instance.GetAllRoles();
                    for (int i = 0; i < roles.Count; i++)
                    {
                        roles[i].UpdateRound();
                    }
                    isHeroTurn = true;
                    _action = new HeroBattleAction(_arrow);
                };

                _roundIndex += 1;
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundOver_Event, new object[] { _roundIndex, callback });
            }
        }

        private void HandleFightEvents(params object[] args)
        {
            if (null == args)
                return;
            if (args.Length <= 0)
                return;

            var strs = ((string)args[0]).Split('_');
            var senderID = (int)args[1];
            if (0 == senderID)
                return;

            var sender = RoleManager.Instance.GetRole(senderID);
            string stateName = strs[0], secondStateName = strs[1];
            sender.HandleEvent(stateName, secondStateName);

            if (null != _action)
                _action.HandleFightEvents(senderID, stateName, secondStateName);
        }

        private void OnSkipRound(params object[] args)
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            foreach (var v in heros)
                v.SetState(Enum.RoleState.Over);

            OnFinishAction();
        }
    }
}