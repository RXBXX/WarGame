using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using FairyGUI;

namespace WarGame
{
    public class BattleField
    {
        private bool _loading = false;
        private bool _loaded = false;
        private int _levelID;
        private BattleAction _action;
        private LocatingArrow _arrow;
        private Coroutine _coroutine;
        private int _touchingID = 0;
        protected string _touchingHexagon = null;
        private List<int> _bornEffects = new List<int>();
        private List<GameObject> _bornEffectGOs = new List<GameObject>();
        private LevelData _levelData = null;
        private int _battleActionID = -1;
        public Weather weather;
        private bool _isLockingCamera;
        public Light mainLight;
        public long startTime;

        public BattleField(int levelID, bool restart)
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Action_Over, OnActionEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Skip_Rount, OnSkipRound);
            EventDispatcher.Instance.AddListener(Enum.EventType.Save_Data, OnSave);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Show_HP, OnShowHP);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Close_HP, OnCloseHP);

            _levelID = levelID;
            _levelData = DatasMgr.Instance.GetLevelData(_levelID);
            if (restart)
            {
                _levelData.Clear();
            }

            Init();
        }

        private void Init()
        {
            UIManager.Instance.OpenPanel("Load", "LoadPanel");

            if (_levelData.Stage == Enum.LevelStage.Passed)
            {
                OnSuccess();
                return;
            }

            var mapDir = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).Map;
            LevelMapPlugin levelPlugin = Tool.Instance.ReadJson<LevelMapPlugin>(mapDir);
            MapManager.Instance.CreateMap(levelPlugin.hexagons, levelPlugin.bonfires);
            if (_levelData.Stage < Enum.LevelStage.Entered)
            {
                var heroDatas = DatasMgr.Instance.GetAllRoles();
                var bornPoints = MapManager.Instance.GetHexagonsByType(Enum.HexagonType.Born);
                var index = 0;
                foreach (var p in bornPoints)
                {
                    _bornEffects.Add(AssetsMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Effects/CFX3_MagicAura_B_Runic.prefab", (GameObject prefab) =>
                    {
                        DebugManager.Instance.Log(prefab.name);
                        var go = GameObject.Instantiate<GameObject>(prefab);
                        go.transform.position = MapManager.Instance.GetHexagon(p).GetPosition() + new Vector3(0.0f, 0.224f, 0.0f);

                        _bornEffectGOs.Add(go);
                    }));
                    var roleData = DatasMgr.Instance.GetRoleData(heroDatas[index++]);
                    var levelRoleData = DatasMgr.Instance.CreateLevelRoleData(Enum.RoleType.Hero, roleData.UID, p);
                    RoleManager.Instance.CreateHero(levelRoleData);
                    _levelData.heros.Add(levelRoleData);
                }

                _levelData.enemys = RoleManager.Instance.InitLevelRoles(levelPlugin.enemys);

                _levelData.Stage = Enum.LevelStage.Entered;
            }
            else
            {
                if (_levelData.Stage < Enum.LevelStage.Readyed)
                {
                    var bornPoints = MapManager.Instance.GetHexagonsByType(Enum.HexagonType.Born);
                    foreach (var p in bornPoints)
                    {
                        _bornEffects.Add(AssetsMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Effects/CFX3_MagicAura_B_Runic.prefab", (GameObject prefab) =>
                        {
                            DebugManager.Instance.Log(prefab.name);
                            var go = GameObject.Instantiate<GameObject>(prefab);
                            go.transform.position = MapManager.Instance.GetHexagon(p).GetPosition() + new Vector3(0.0f, 0.224f, 0.0f);

                            _bornEffectGOs.Add(go);
                        }));
                    }
                }

                foreach (var v in _levelData.heros)
                {
                    if (v.HP > 0)
                        RoleManager.Instance.CreateHero(v);
                }

                foreach (var v in _levelData.enemys)
                {
                    if (v.HP > 0)
                        RoleManager.Instance.CreateEnemy(v);
                }
            }

            weather = new Weather();
            _arrow = new LocatingArrow();
            mainLight = GameObject.Find("Directional Light").GetComponent<Light>();

            _loading = true;
            startTime = TimeMgr.Instance.GetUnixTimestamp();
            DebugManager.Instance.Log(TimeMgr.Instance.GetUnixTimestamp());
        }

        public void Update(float deltaTime)
        {
            if (_loading)
            {
                var progress = (RoleManager.Instance.GetLoadingProgress() + MapManager.Instance.GetLoadingProgress() + _arrow.GetLoadingProgress()) / 3;
                EventDispatcher.Instance.PostEvent(Enum.EventType.Scene_Load_Progress, new object[] { progress });

                if (progress >= 1 && null == _coroutine)
                {
                    _loading = false;
                    //延迟一帧开始，防止场景内对象没有初始化完成
                    _coroutine = CoroutineMgr.Instance.StartCoroutine(OnLoad());
                }
            }

            if (_loaded)
            {
                if (null != _action)
                    _action.Update(deltaTime);

                if (null != weather)
                    weather.Update(deltaTime);

                MapManager.Instance.UpdateHexagon(weather.GetLightIntensity());
            }
        }

        public void Dispose()
        {
            UIManager.Instance.ClosePanel("FightPanel");
            if (null != weather)
            {
                weather.Dispose();
                weather = null;
            }

            if (null != _arrow)
            {
                _arrow.Dispose();
                _arrow = null;
            }

            ClearBornEffects();
            CameraMgr.Instance.SetTarget(0);

            DisposeAction(_battleActionID, true);

            Tool.ClearAppliedProcessingFotOutLine();
            RoleManager.Instance.Clear();
            MapManager.Instance.ClearMap();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Action_Over, OnActionEnd);
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
            yield return new WaitForSeconds(0.2F);
            UIManager.Instance.ClosePanel("LoadPanel");
            UIManager.Instance.OpenPanel("Fight", "FightPanel", new object[] { _levelData.Stage >= Enum.LevelStage.Readyed, _levelData.Round });

            if (_levelData.Stage < Enum.LevelStage.Talked)
            {
                var dialogGroup = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelData.configId).StartDialog;
                DialogMgr.Instance.OpenDialog(dialogGroup, (args) =>
                {
                    _levelData.Stage = Enum.LevelStage.Talked;
                    _action = new ReadyBattleAction(GetActionID(), _levelData);
                });
            }
            else if (_levelData.Stage < Enum.LevelStage.Readyed)
            {
                _action = new ReadyBattleAction(GetActionID(), _levelData);
            }
            else
            {
                NextAction();
            }

            DebugManager.Instance.Log(TimeMgr.Instance.GetUnixTimestamp());

            DebugManager.Instance.Log("进入战场共耗时：" + (TimeMgr.Instance.GetUnixTimestamp() - startTime));

            //DebugManager.Instance.Log(CameraMgr.Instance.MainCamera.name);
            //CameraMgr.Instance.MainCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        public void FocusIn(GameObject obj)
        {
            if (!_loaded)
                return;

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

            if (null != _action)
            {
                _action.FocusIn(obj);
            }
        }

        public void ClickBegin(GameObject obj)
        {
            if (!_loaded)
                return;

            if (null != _action)
            {
                _action.OnClickBegin(obj);
            }
        }

        public void ClickEnd()
        {
            if (!_loaded)
                return;

            if (null != _action)
            {
                _action.OnClickEnd();
            }
        }

        public void Click(GameObject obj)
        {
            if (!_loaded)
                return;

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

            if (null != _action)
            {
                _action.OnClick(obj);
            }
        }

        public void RightClickBegin(GameObject obj)
        {
            var tag = obj.tag;
            if (tag == Enum.Tag.Hero.ToString() || tag == Enum.Tag.Enemy.ToString())
            {
                _isLockingCamera = CameraMgr.Instance.Lock();

                var screenPos = InputManager.Instance.GetMousePos();
                screenPos.y = Screen.height - screenPos.y;
                var uiPos = GRoot.inst.GlobalToLocal(screenPos);
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_RoleInfo, new object[] { uiPos, obj.GetComponent<RoleBehaviour>().ID });
            }
        }

        public void RightClickEnd()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Hide_RoleInfo);
            if (_isLockingCamera)
            {
                CameraMgr.Instance.Unlock();
                _isLockingCamera = false;
            }
        }

        private int GetActionID()
        {
            return ++_battleActionID;
        }

        private void DisposeAction(int actionID, bool save = false)
        {
            if (null == _action)
                return;
            if (_action.ID != actionID)
                return;
            _action.Dispose(save);
            _action = null;
        }

        private void OnActionEnd(params object[] args)
        {
            RoleManager.Instance.ClearDeadRole();

            if (null != args && args.Length > 0)
            {
                DisposeAction((int)args[0]);

                if ((int)args[0] == 0)
                {
                    ClearBornEffects();
                }
            }

            NextAction();
        }

        private void NextAction()
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            if (heros.Count <= 0)
            {
                DialogMgr.Instance.OpenDialog(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).FailedDialog, (args) =>
                {
                    OnFailed();
                });
                return;
            }

            var enemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);
            if (enemys.Count <= 0)
            {
                DialogMgr.Instance.OpenDialog(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).WinDialog, (args) =>
                {
                    OnSuccess();
                });
                return;
            }

            if (_levelData.actionType == Enum.ActionType.HeroAction)
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

                BattleRoundFunc callback = () =>
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
                    _levelData.actionType = Enum.ActionType.EnemyAction;
                };

                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundChange_Event, new object[] { Enum.FightTurn.EnemyTurn, callback });
            }

            if (_levelData.actionType == Enum.ActionType.EnemyAction)
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

                _levelData.Round++;
                BattleRoundFunc callback = () =>
                {
                    MapManager.Instance.UpdateRound(_levelData.Round);
                    RoleManager.Instance.UpdateRound(_levelData.Round);
                    _levelData.actionType = Enum.ActionType.HeroAction;
                    _action = new HeroBattleAction(GetActionID());
                };

                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundChange_Event, new object[] { Enum.FightTurn.HeroTurn, callback });
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_RoundOver_Event, new object[] { _levelData.Round, callback });
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

            OnActionEnd(new object[] { _battleActionID });
        }

        private void ClearBornEffects()
        {
            foreach (var v in _bornEffectGOs)
            {
                AssetsMgr.Instance.Destroy(v);
            }
            _bornEffectGOs.Clear();

            foreach (var v in _bornEffects)
            {
                AssetsMgr.Instance.ReleaseAsset(v);
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
        }

        private void OnCloseHP(params object[] args)
        {
            UIManager.Instance.ClosePanel("FightArenaPanel");
        }

        private void OnSuccess()
        {
            _levelData.Stage = Enum.LevelStage.Passed;

            DatasMgr.Instance.AddItems(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).Rewards);

            OnSave();
            UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { true, _levelID });
        }

        private void OnFailed()
        {
            UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { false, _levelID });
        }
    }
}