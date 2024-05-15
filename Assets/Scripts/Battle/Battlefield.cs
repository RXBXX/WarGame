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
        private int _roundIndex = 0;
        private bool isHeroTurn = true;
        private BattleAction _action;

        public BattleField(int levelID)
        {
            _levelID = levelID;

            DatasMgr.Instance.StartLevel(levelID);

            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Action_Over, OnFinishAction);

            MapManager.Instance.CreateMap(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID).Map);

            var time = Time.realtimeSinceStartup;

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

            roleData = DatasMgr.Instance.GetRoleData(3);
            levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, roleData.equipmentDic, roleData.skillDic);
            levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
            levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(1, 0, 1));
            RoleManager.Instance.CreateHero(levelRoleData);

            roleData = DatasMgr.Instance.GetRoleData(4);
            levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, roleData.equipmentDic, roleData.skillDic);
            levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
            levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(2, 0, 1));
            RoleManager.Instance.CreateHero(levelRoleData);

            levelRoleData = new LevelRoleData(11, 10003, 1, new Dictionary<Enum.EquipPlace, int>(), new Dictionary<int, int>());
            levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", 10004 * 1000 + 1).HP;
            levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 3));
            RoleManager.Instance.CreateEnemy(levelRoleData);

            levelRoleData = new LevelRoleData(12, 10004, 1, new Dictionary<Enum.EquipPlace, int>(), new Dictionary<int, int>());
            levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", 10004 * 1000 + 1).HP;
            levelRoleData.hexagonID = MapTool.Instance.GetHexagonKey(new Vector3(4, 0, 5));
            RoleManager.Instance.CreateEnemy(levelRoleData);
            DebugManager.Instance.Log("Duration: " + (Time.realtimeSinceStartup - time));
            UIManager.Instance.OpenPanel("Fight", "FightPanel");
        }

        public void Dispose()
        {
            _action.Dispose();
            RoleManager.Instance.Clear();
            MapManager.Instance.ClearMap();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Action_Over, OnFinishAction);
        }

        public void Start()
        {
            var roles = RoleManager.Instance.GetAllRoles();
            for (int i = roles.Count - 1; i >= 0; i--)
                roles[i].UpdateRound();

            _action = new HeroBattleAction();
        }

        public void Touch(GameObject obj)
        {
            if (null == _action)
                return;
            _action.OnTouch(obj);
        }

        public void Click(GameObject obj)
        {
            if (null == _action)
                return;
            _action.OnClick(obj);
        }

        private void DisposeAction()
        {
            DebugManager.Instance.Log("DisposeAction");
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
                        _action = new HeroBattleAction();
                        return;
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
                    var roles = RoleManager.Instance.GetAllRoles();
                    for (int i = 0; i < roles.Count; i++)
                    {
                        roles[i].UpdateRound();
                    }
                     isHeroTurn = true;
                    _action = new HeroBattleAction();
                };

                _roundIndex += 1;
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundOver_Event, new object[] { _roundIndex, callback });
            }
        }
    }
}