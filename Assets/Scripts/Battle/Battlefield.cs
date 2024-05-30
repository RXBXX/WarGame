using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public delegate void RoundFunc();

    public class BattleField
    {
        private bool _loaded = false;
        private int _levelID;
        private int _roundIndex = 0;
        private bool isHeroTurn = true;
        private BattleAction _action;
        private LocatingArrow _arrow;
        private Coroutine _coroutine;
        private int _touchingID = 0;
        protected string _touchingHexagon = null;
        private List<int> _bornEffects = new List<int>();
        private List<GameObject> _bornEffectGOs = new List<GameObject>();
        private LevelData _levelData = null;
        private int _battleActionID = -1;

        public BattleField(int levelID)
        {
            UIManager.Instance.OpenPanel("Load", "LoadPanel");

            _levelID = levelID;
            _levelData = DatasMgr.Instance.GetLevelData(_levelID).Clone();

            DatasMgr.Instance.StartLevel(levelID);

            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Action_Over, OnFinishAction);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Skip_Rount, OnSkipRound);
            EventDispatcher.Instance.AddListener(Enum.EventType.Save_Data, OnSave);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Show_HP, OnShowHP);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Close_HP, OnCloseHP);

            var mapDir = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID).Map;
            LevelMapPlugin levelPlugin = Tool.Instance.ReadJson<LevelMapPlugin>(mapDir);

            MapManager.Instance.CreateMap(levelPlugin.hexagons);

            var heroDatas = DatasMgr.Instance.GetAllRoles();
            if (_levelData.heros.Count <= 0)
            {
                var bornPoints = MapManager.Instance.GetHexagonsByType(Enum.HexagonType.Born);
                var index = 0;
                foreach (var p in bornPoints)
                {
                    _bornEffects.Add(AssetMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Effects/CFX3_MagicAura_B_Runic.prefab", (GameObject prefab) =>
                    {
                        DebugManager.Instance.Log(prefab.name);
                        var go = GameObject.Instantiate<GameObject>(prefab);
                        go.transform.position = MapManager.Instance.GetHexagon(p).GetPosition() + new Vector3(0.0f, 0.224f, 0.0f);

                        _bornEffectGOs.Add(go);
                    }));

                    var roleData = DatasMgr.Instance.GetRoleData(heroDatas[index]);
                    var equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                    foreach (var v in roleData.equipmentDic)
                    {
                        equipDataDic.Add(v.Key, DatasMgr.Instance.GetEquipmentData(v.Value));
                    }
                    var levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, Enum.RoleState.Waiting, equipDataDic, roleData.talentDic);
                    levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
                    levelRoleData.hexagonID = p;
                    RoleManager.Instance.CreateHero(levelRoleData);
                    _levelData.heros.Add(levelRoleData);

                    index += 1;
                }
            }
            else
            {
                foreach (var v in _levelData.heros)
                {
                    RoleManager.Instance.CreateHero(v);
                }
            }

            if (_levelData.enemys.Count <= 0)
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
                    _levelData.enemys.Add(levelRoleData);
                }
            }
            else
            {
                foreach (var v in _levelData.enemys)
                {
                    RoleManager.Instance.CreateEnemy(v);
                }
            }

            _arrow = new LocatingArrow();
        }

        public void Update(float deltaTime)
        {
            if (_loaded)
            {
                if (null != _action)
                    _action.Update(deltaTime);
                return;
            }
            var progress = (RoleManager.Instance.GetLoadingProgress() + MapManager.Instance.GetLoadingProgress() + _arrow.GetLoadingProgress()) / 3;
            EventDispatcher.Instance.PostEvent(Enum.EventType.Scene_Load_Progress, new object[] { progress });

            if (progress >= 1 && null == _coroutine)
            {
                //延迟一帧开始，防止场景内对象没有初始化完成
                _coroutine = CoroutineMgr.Instance.StartCoroutine(OnLoad());
            }


        }

        public void Dispose()
        {
            ClearBornEffects();
            CameraMgr.Instance.SetTarget(0);

            DisposeAction(_battleActionID);

            RoleManager.Instance.Clear();
            MapManager.Instance.ClearMap();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Action_Over, OnFinishAction);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Skip_Rount, OnSkipRound);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Save_Data, OnSave);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Show_HP, OnShowHP);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Close_HP, OnCloseHP);
        }

        private IEnumerator OnLoad()
        {
            yield return null;
            _loaded = true;

            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            CameraMgr.Instance.SetTarget(heros[0].ID);
            UIManager.Instance.ClosePanel("LoadPanel");
            UIManager.Instance.OpenPanel("Fight", "FightPanel", new object[] { _levelData.isReady});

            if (!_levelData.isRead)
            {
                var dialogGroup = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelData.configId).StartDialog;
                DialogMgr.Instance.OpenDialog(dialogGroup, (args) =>
                {
                    _levelData.isRead = true;
                    if (!_levelData.isReady)
                        _action = new ReadyBattleAction(GetActionID(), _levelData);
                });
            }
            else if (!_levelData.isReady)
            {
                _action = new ReadyBattleAction(GetActionID(), _levelData);
            }
            else
            {
                _action = new HeroBattleAction(GetActionID());
            }
        }

        public void Touch(GameObject obj)
        {
            if (!_loaded)
                return;

            OnTouch(obj);

            if (null != _action)
            {
                _action.OnTouch(obj);
            }
        }

        private void OnTouch(GameObject obj)
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
                var role = RoleManager.Instance.GetRole(_touchingID);
                if (null != role)
                    role.ResetHighLight();
                _touchingID = 0;

            }
            if (0 != touchingID && _touchingID != touchingID)
            {
                _touchingID = touchingID;
                var role = RoleManager.Instance.GetRole(_touchingID);
                if (null != role)
                    role.HighLight();
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

        public void ClickBegin(GameObject obj)
        {
            if (!_loaded)
                return;

            //OnClickBegin(obj);

            if (null != _action)
            {
                _action.OnClickBegin(obj);
            }
        }

        public void OnClickBegin(GameObject obj)
        {
            var tag = obj.tag;
            if (tag == Enum.Tag.Hero.ToString())
            {
                CameraMgr.Instance.SetTarget(obj.GetComponent<RoleBehaviour>().ID);
            }
            else if (tag == Enum.Tag.Enemy.ToString())
            {
                CameraMgr.Instance.SetTarget(obj.GetComponent<RoleBehaviour>().ID);
            }
            else if (tag == Enum.Tag.Hexagon.ToString())
            {
                var hexagonID = obj.GetComponent<HexagonBehaviour>().ID;

                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
                if (roleID > 0)
                {
                    CameraMgr.Instance.SetTarget(roleID);
                }
            }
        }

        public void Click(GameObject obj)
        {
            if (!_loaded)
                return;

            OnClickBegin(obj);

            if (null != _action)
            {
                _action.OnClick(obj);
            }
        }

        private int GetActionID()
        {
            return ++_battleActionID;
        }

        private void DisposeAction(int actionID)
        {
            if (null == _action)
                return;
            if (_action.ID != actionID)
                return;
            _action.Dispose();
            _action = null;
        }

        private void OnFinishAction(params object[] args)
        {
            DebugManager.Instance.Log("OnFinishAction");
            DisposeAction((int)args[0]);

            if ((int)args[0] == 0)
            {
                ClearBornEffects();
            }

            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            if (heros.Count <= 0)
            {
                DialogMgr.Instance.OpenDialog(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).FailedDialog, (args) =>
                {
                    UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { false });
                });
                return;
            }

            var enemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);
            if (enemys.Count <= 0)
            {
                DatasMgr.Instance.CompleteLevel(_levelID);
                DialogMgr.Instance.OpenDialog(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).WinDialog, (args) =>
                {
                    UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { true });
                });
                return;
            }

            if (isHeroTurn)
            {
                //检测所有英雄是否行动结束
                for (int i = heros.Count - 1; i >= 0; i--)
                {
                    if (heros[i].GetState() != Enum.RoleState.Over)
                    {
                        _action = new HeroBattleAction(GetActionID());
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
                            _action = new EnemyBattleAction(GetActionID());
                            enemys[i].SetState(Enum.RoleState.Waiting);
                            break;
                        }
                    }
                    isHeroTurn = false;
                };

                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundChange_Event, new object[] { Enum.FightTurn.EnemyTurn, callback });
            }

            if (!isHeroTurn)
            {
                //查找到下一个应该行动的敌人
                for (int i = enemys.Count - 1; i >= 0; i--)
                {
                    if (enemys[i].GetState() == Enum.RoleState.Locked)
                    {
                        _action = new EnemyBattleAction(GetActionID());
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
                    _action = new HeroBattleAction(GetActionID());
                };

                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundChange_Event, new object[] { Enum.FightTurn.HeroTurn, callback });

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

            OnFinishAction(new object[] { _battleActionID });
        }

        private void ClearBornEffects()
        {
            foreach (var v in _bornEffectGOs)
            {
                AssetMgr.Instance.Destroy(v);
            }
            _bornEffectGOs.Clear();

            foreach (var v in _bornEffects)
            {
                AssetMgr.Instance.ReleaseAsset(v);
            }
            _bornEffects.Clear();
        }

        private void OnSave(params object[] args)
        {
            DatasMgr.Instance.SetLevelData(_levelData.Clone());
        }

        private void OnShowHP(params object[] args)
        {
            UIManager.Instance.OpenPanel("Fight", "FightArenaPanel", args);
            //var initiator = RoleManager.Instance.GetRole((int)args[0]);
            //var target = RoleManager.Instance.GetRole((int)args[1]);
            //_initiatorHP.GetController("style").SetSelectedIndex(initiator.Type == Enum.RoleType.Hero ? 0 : 1);
            //_targetHP.GetController("style").SetSelectedIndex(target.Type == Enum.RoleType.Hero ? 0 : 1);
            //_initiatorHP.value = initiator.GetHP();
            //_targetHP.value = target.GetHP();
            //_initiatorHP.visible = true;
            //_targetHP.visible = true;
            //_showHP.Play();
        }

        private void OnCloseHP(params object[] args)
        {
            UIManager.Instance.ClosePanel("FightArenaPanel");
            //_showHP.PlayReverse(() =>
            //{
            //    _initiatorHP.visible = false;
            //    _targetHP.visible = false;
            //});
        }
    }
}