using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using FairyGUI;
using DG.Tweening;

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
        protected int _touchingHexagon = -1;
        private List<int> _bornEffects = new List<int>();
        private List<GameObject> _bornEffectGOs = new List<GameObject>();
        private LevelData _levelData = null;
        private int _battleActionID = -1;
        public Weather weather;
        private bool _isLockingCamera;
        public Light mainLight;
        private List<int> _assets = new List<int>();
        private List<Sequence> _sequences = new List<Sequence>();
        private List<GameObject> _gos = new List<GameObject>();
        private LevelMapPlugin _levelPlugin;
        //public long startTime;

        public BattleField(int levelID, bool restart)
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Action_Over, OnActionEnd);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Skip_Rount, OnSkipRound);
            EventDispatcher.Instance.AddListener(Enum.Event.Save_Data, OnSave);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Show_HP, OnShowHP);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Close_HP, OnCloseHP);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Drops, OnFightDrops);

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
            BattleMgr.Instance.InitReports(_levelData.reportDic);

            if (_levelData.Stage == Enum.LevelStage.Passed)
            {
                OnSuccess();
                return;
            }
            else if (_levelData.Stage == Enum.LevelStage.Failed)
            {
                OnFailed();
                return;
            }

            var mapDir = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).Map;
            _levelPlugin = Tool.Instance.ReadJson<LevelMapPlugin>(Application.streamingAssetsPath + "/" + mapDir);
            MapManager.Instance.CreateMap(_levelPlugin.hexagons, _levelPlugin.bonfires, _levelPlugin.ornaments, _levelPlugin.lightingPlugin);
            if (_levelData.Stage < Enum.LevelStage.Entered)
            {
                var heroDatas = DatasMgr.Instance.GetAllRoles();
                var bornPoints = MapManager.Instance.GetHexagonsByType(Enum.HexagonType.Hex22);
                var selectedHeros = DatasMgr.Instance.GetSelectedHeros();
                for (int i = 0; i < bornPoints.Count; i++)
                {
                    var pos = MapManager.Instance.GetHexagon(bornPoints[i]).GetPosition() + new Vector3(0.0f, 0.224f, 0.0f);
                    _bornEffects.Add(AssetsMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Effects/CFX3_MagicAura_B_Runic.prefab", (GameObject prefab) =>
                    {
                        var go = GameObject.Instantiate<GameObject>(prefab);
                        go.transform.position = pos;
                        _bornEffectGOs.Add(go);
                    }));

                    RoleData roleData;
                    if (null != selectedHeros && i < selectedHeros.Count)
                    {
                        roleData = DatasMgr.Instance.GetRoleData(selectedHeros[i]);
                        heroDatas.Remove(roleData.UID);
                    }
                    else if (null != selectedHeros)
                    {
                        roleData = DatasMgr.Instance.GetRoleData(heroDatas[i - selectedHeros.Count]);
                    }
                    else
                    {
                        roleData = DatasMgr.Instance.GetRoleData(heroDatas[i]);
                    }

                    var levelRoleData = Factory.Instance.GetLevelRoleData(Enum.RoleType.Hero, roleData.UID, bornPoints[i]);
                    //DebugManager.Instance.Log(levelRoleData.HP + "_" + levelRoleData.GetAttribute(Enum.AttrType.HP));
                    var role = RoleManager.Instance.CreateRole(Enum.RoleType.Hero, levelRoleData);
                    role.GoIntoBattle();
                    _levelData.heros.Add(levelRoleData);
                }

                _levelData.enemys = RoleManager.Instance.InitLevelRoles(_levelPlugin.enemys);
                _levelData.Stage = Enum.LevelStage.Entered;
            }
            else
            {
                if (_levelData.Stage < Enum.LevelStage.Readyed)
                {
                    var bornPoints = MapManager.Instance.GetHexagonsByType(Enum.HexagonType.Hex22);
                    foreach (var p in bornPoints)
                    {
                        _bornEffects.Add(AssetsMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Effects/CFX3_MagicAura_B_Runic.prefab", (GameObject prefab) =>
                        {
                            var go = GameObject.Instantiate<GameObject>(prefab);
                            go.transform.position = MapManager.Instance.GetHexagon(p).GetPosition() + new Vector3(0.0f, 0.224f, 0.0f);

                            _bornEffectGOs.Add(go);
                        }));
                    }
                }

                foreach (var v in _levelData.heros)
                {
                    if (v.HP > 0)
                        RoleManager.Instance.CreateRole(Enum.RoleType.Hero, v);
                }

                foreach (var v in _levelData.enemys)
                {
                    if (v.HP > 0)
                        RoleManager.Instance.CreateRole(Enum.RoleType.Enemy, v);
                }
            }

            weather = new Weather();
            _arrow = new LocatingArrow();
            mainLight = GameObject.Find("Directional Light").GetComponent<Light>();

            _loading = true;
            //startTime = TimeMgr.Instance.GetUnixTimestamp();
            //DebugManager.Instance.Log(TimeMgr.Instance.GetUnixTimestamp());
        }

        public void Update(float deltaTime)
        {
            if (_loading)
            {
                var progress = (RoleManager.Instance.GetLoadingProgress() + MapManager.Instance.GetLoadingProgress() + _arrow.GetLoadingProgress()) / 3;
                EventDispatcher.Instance.PostEvent(Enum.Event.Scene_Load_Progress, new object[] { progress });

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

                //MapManager.Instance.UpdateHexagon(weather.GetLightIntensity());
            }
        }

        public void Dispose()
        {
            UIManager.Instance.ClosePanel("FightPanel");

            AudioMgr.Instance.PlayMusic("Assets/Audios/BG_Music.mp3");

            RenderMgr.Instance.ClosePostProcessiong(Enum.PostProcessingType.Fog);

            RenderMgr.Instance.ClosePostProcessiong(Enum.PostProcessingType.Palette);

            CameraMgr.Instance.StopFloatPoint();

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
            DisposeAction(_battleActionID, true);

            CameraMgr.Instance.SetTarget(0);

            Tool.ClearAppliedProcessingFotOutLine();
            RoleManager.Instance.Clear();
            MapManager.Instance.ClearMap();

            foreach (var v in _gos)
            {
                AudioMgr.Instance.ClearSound(v);
                AssetsMgr.Instance.Destroy(v);
            }
            _gos.Clear();

            foreach (var v in _assets)
                AssetsMgr.Instance.ReleaseAsset(v);
            _assets.Clear();

            foreach (var v in _sequences)
                v.Kill();
            _sequences.Clear();

            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Action_Over, OnActionEnd);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Event, HandleFightEvents);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Skip_Rount, OnSkipRound);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Save_Data, OnSave);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Show_HP, OnShowHP);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Close_HP, OnCloseHP);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Drops, OnFightDrops);
        }

        private IEnumerator OnLoad()
        {
            yield return null;
            _loaded = true;

            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            CameraMgr.Instance.SetTarget(heros[0].ID, false);
            yield return new WaitForSeconds(0.2F);
            UIManager.Instance.ClosePanel("LoadPanel");
            UIManager.Instance.OpenPanel("Fight", "FightPanel", new object[] { _levelID, _levelData.Stage >= Enum.LevelStage.Readyed, _levelData.Round });

            var levelConfig = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelData.configId);
            if (_levelData.Stage < Enum.LevelStage.Talked)
            {
                CameraMgr.Instance.FloatPoints(_levelPlugin.points, (args) =>
                {
                    EventMgr.Instance.TriggerEvent(levelConfig.StartEvent, (args) =>
                    {
                        _levelData.Stage = Enum.LevelStage.Talked;
                        _levelData.actionType = Enum.ActionType.ReadyAction;
                        _action = new ReadyBattleAction(GetActionID(), _levelID, _levelData);
                    });


                });
            }
            else if (_levelData.Stage < Enum.LevelStage.Readyed)
            {
                _levelData.actionType = Enum.ActionType.ReadyAction;
                _action = new ReadyBattleAction(GetActionID(), _levelID, _levelData);
            }
            else
            {
                NextAction();
            }

            AudioMgr.Instance.PlayMusic(levelConfig.Music);
            RenderMgr.Instance.OpenPostProcessiong(Enum.PostProcessingType.Fog);
            RenderMgr.Instance.OpenPostProcessiong(Enum.PostProcessingType.Palette, new object[] { CommonParams.GetPalette(levelConfig.Element) });
            //DebugManager.Instance.Log(TimeMgr.Instance.GetUnixTimestamp());

            //DebugManager.Instance.Log("进入战场共耗时：" + (TimeMgr.Instance.GetUnixTimestamp() - startTime));

            //DebugManager.Instance.Log(CameraMgr.Instance.MainCamera.name);
            //CameraMgr.Instance.MainCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        public void FocusIn(GameObject obj)
        {
            if (!_loaded)
                return;

            var touchingID = 0;
            int touchingHexagonID = -1;
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
                    if (roleID == 0)
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

            if (-1 == touchingHexagonID)
            {
                if (null != _arrow && _arrow.Active)
                {
                    _arrow.Active = false;
                }
                _touchingHexagon = -1;
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

            //点击角色时，如何切换摄像机目标在视觉上会扰乱操作，目前调整为按下鼠标左键拖拽切换目标
            var tag = obj.tag;
            if (tag == Enum.Tag.Hero.ToString())
            {
                //CameraMgr.Instance.SetTarget(obj.GetComponent<RoleBehaviour>().ID);
            }
            else if (tag == Enum.Tag.Enemy.ToString())
            {
                //CameraMgr.Instance.SetTarget(obj.GetComponent<RoleBehaviour>().ID);
            }
            else if (tag == Enum.Tag.Hexagon.ToString())
            {
                //var hexagonID = obj.GetComponent<HexagonBehaviour>().ID;

                //var roleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
                //if (roleID > 0)
                //{
                //    CameraMgr.Instance.SetTarget(roleID);
                //}
            }

            if (null != _action)
            {
                _action.OnClick(obj);
            }
        }

        public void RightClickBegin(GameObject obj)
        {
            //DebugManager.Instance.Log("RIghtClickBegin");
            if (_isLockingCamera)
                return;

            var tag = obj.tag;
            if (tag == Enum.Tag.Hero.ToString() || tag == Enum.Tag.Enemy.ToString())
            {
                _isLockingCamera = CameraMgr.Instance.Lock();

                var screenPos = InputManager.Instance.GetMousePos();
                screenPos.y = Screen.height - screenPos.y;
                var uiPos = GRoot.inst.GlobalToLocal(screenPos);
                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_RoleInfo, new object[] { uiPos, obj.GetComponent<RoleBehaviour>().ID, _levelID });
            }
        }

        public void RightClickEnd()
        {
            //DebugManager.Instance.Log("RightClickEnd");
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Hide_RoleInfo);
            if (_isLockingCamera)
            {
                CameraMgr.Instance.Unlock();
                _isLockingCamera = false;
            }
        }

        private int GetActionID()
        {
            var id = ++_battleActionID;
            //DebugManager.Instance.Log("ActionID:"+id);
            return id;
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
            //DebugManager.Instance.Log("OnActionEnd");
            if (null != args && args.Length > 0)
            {
                DisposeAction((int)args[0]);

                if ((int)args[0] == 0)
                {
                    ClearBornEffects();
                }
            }

            ClearDeadRole((args) =>
            {
                if (IsOver())
                    return;

                NextAction();
            });
        }

        private void NextAction(params object[] args)
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            var enemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);

            if (_levelData.actionType == Enum.ActionType.ReadyAction)
            {
                _levelData.actionType = Enum.ActionType.HeroAction;
                _action = new HeroBattleAction(GetActionID(), _levelID);
                return;
            }

            if (_levelData.actionType == Enum.ActionType.HeroAction)
            {
                //检测所有英雄是否行动结束
                for (int i = heros.Count - 1; i >= 0; i--)
                {
                    if (heros[i].CanAction())
                    {
                        _action = new HeroBattleAction(GetActionID(), _levelID);
                        return;
                    }
                }

                BattleRoundFunc callback = () =>
                {
                    //DebugManager.Instance.Log("BattleRoundFunc");
                    _levelData.actionType = Enum.ActionType.EnemyAction;
                    foreach (var v in enemys)
                    {
                        v.UpdateRound(Enum.RoleType.Enemy);
                    }
                    foreach (var v in heros)
                    {
                        v.UpdateRound(Enum.RoleType.Enemy);
                    }

                    //查找到下一个应该行动的敌人
                    for (int i = enemys.Count - 1; i >= 0; i--)
                    {
                        //if (enemys[i].GetState() == Enum.RoleState.Locked)
                        if (enemys[i].CanAction())
                        {
                            _action = new EnemyBattleAction(GetActionID(), _levelID);
                            enemys[i].SetState(Enum.RoleState.Waiting);
                            break;
                        }
                    }

                    if (null == _action)
                        NextAction();
                };

                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_RoundChange_Event, new object[] { Enum.FightTurn.EnemyTurn, callback });
            }

            if (_levelData.actionType == Enum.ActionType.EnemyAction)
            {
                //查找到下一个应该行动的敌人
                for (int i = enemys.Count - 1; i >= 0; i--)
                {
                    //if (enemys[i].GetState() == Enum.RoleState.Locked)
                    if (enemys[i].CanAction())
                    {
                        _action = new EnemyBattleAction(GetActionID(), _levelID);
                        enemys[i].SetState(Enum.RoleState.Waiting);
                        return;
                    }
                }

                _levelData.Round++;
                BattleRoundFunc callback = () =>
                {
                    _levelData.actionType = Enum.ActionType.HeroAction;
                    MapManager.Instance.UpdateRound(_levelData.Round);

                    foreach (var v in enemys)
                    {
                        v.UpdateRound(Enum.RoleType.Hero);
                    }
                    foreach (var v in heros)
                    {
                        v.UpdateRound(Enum.RoleType.Hero);
                    }

                    CoroutineMgr.Instance.StartCoroutine(OnUpdateRound());
                };

                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_RoundChange_Event, new object[] { Enum.FightTurn.HeroTurn, callback });
                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_RoundOver_Event, new object[] { _levelData.Round });
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
            if (null == sender)
                return;

            string stateName = strs[0], secondStateName = strs[1];
            //DebugManager.Instance.Log(stateName + "+" + secondStateName);
            sender.HandleEvent(stateName, secondStateName);

            if (null != _action)
                _action.HandleFightEvents(senderID, stateName, secondStateName);
        }

        private void OnSkipRound(params object[] args)
        {
            if (null == _action)
                return;

            if (_action.Type != Enum.ActionType.HeroAction)
                return;

            if (!_action.CanSkip())
            {
                TipsMgr.Instance.Add(ConfigMgr.Instance.GetTranslation("FightPanel_SkipTips"));
                return;
            }

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
            DatasMgr.Instance.Save();
            //DatasMgr.Instance.SetLevelData(_levelData.Clone());
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
            DebugManager.Instance.Log("MinPassRound:" + _levelData.Round);
            if (0 == _levelData.minPassRound || _levelData.minPassRound > _levelData.Round)
                _levelData.minPassRound = _levelData.Round;

            foreach (var v in _levelData.itemsDic)
            {
                DatasMgr.Instance.AddItem(v.Key, v.Value);
            }
            _levelData.itemsDic.Clear();

            OnSave();
            var reportDic = BattleMgr.Instance.GetReports();
            BattleMgr.Instance.ClearReports();
            UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { true, _levelID, reportDic });
        }

        private void OnFailed()
        {
            _levelData.Stage = Enum.LevelStage.Failed;
            OnSave();

            var reportDic = BattleMgr.Instance.GetReports();
            BattleMgr.Instance.ClearReports();
            UIManager.Instance.OpenPanel("Fight", "FightOverPanel", new object[] { false, _levelID, reportDic});
        }

        //回合结束，清理死亡的角色
        private void ClearDeadRole(WGArgsCallback callback = null)
        {
            var deadRoles = RoleManager.Instance.ClearDeadRole();
            var events = new List<int>();
            if (deadRoles.Count > 0)
            {
                for (int i = deadRoles.Count - 1; i >= 0; i--)
                {
                    //DebugManager.Instance.Log("Dead:" + deadRoles[i]);
                    var enemyConfig = ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", deadRoles[i]);
                    if (null != enemyConfig && 0 != enemyConfig.DefeatEvent)
                    {
                        events.Add(enemyConfig.DefeatEvent);
                    }
                }
            }

            HandleDefeatEvent(events, callback);
        }

        //执行死亡角色事件
        private void HandleDefeatEvent(List<int> events, WGArgsCallback callback = null)
        {
            if (events.Count > 0)
            {
                EventMgr.Instance.TriggerEvent(events[0], (args) =>
                {
                    HandleDefeatEvent(events, callback);
                });
                events.RemoveAt(0);
            }
            else if (null != callback)
            {
                callback();
            }
        }

        //判断关卡是否结束
        private bool IsOver()
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            if (heros.Count <= 0)
            {
                EventMgr.Instance.TriggerEvent(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).FailedEvent, (args) =>
                {
                    OnFailed();
                });
                return true;
            }

            var enemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);
            if (enemys.Count <= 0)
            {
                EventMgr.Instance.TriggerEvent(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).WinEvent, (args) =>
                {
                    var itemsDic = new List<TwoIntPair>();
                    foreach (var v in _levelData.itemsDic)
                        itemsDic.Add(new TwoIntPair(v.Key, v.Value));
                    WGCallback cb = ()=>{ OnSuccess(); };
                    UIManager.Instance.OpenPanel("Reward", "RewardItemsPanel", new object[] {itemsDic, cb });
                });
                return true;
            }

            return false;
        }

        private IEnumerator OnUpdateRound()
        {
            var haveDead = false;
            foreach (var v in RoleManager.Instance.GetAllRoles())
            {
                if (v.IsDead())
                {
                    haveDead = true;
                    break;
                }
            }

            if (haveDead)
                yield return new WaitForSeconds(0.8f);
            else
                yield return null;

            ClearDeadRole((args) =>
            {
                if (IsOver())
                    return;

                _levelData.actionType = Enum.ActionType.HeroAction;

                var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
                foreach (var v in heros)
                {
                    if (v.CanAction())
                    {
                        _action = new HeroBattleAction(GetActionID(), _levelID);
                        break;
                    }
                }
                if (null == _action)
                    NextAction();
            });
        }

        private void OnFightDrops(params object[] args)
        {
            var reward = (int)args[0];
            var pos = (Vector3)args[1];
            var rewardConfig = ConfigMgr.Instance.GetConfig<RewardConfig>("RewardConfig", reward);

            foreach (var v in rewardConfig.Rewards)
                _levelData.AddItem(v.id, v.value);

            foreach (var v in rewardConfig.Rewards)
            {
                var itemConfig = ConfigMgr.Instance.GetConfig<ItemConfig>("ItemConfig", v.id);
                int assetID = 0;
                assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(itemConfig.Prefab, (prefab) =>
                {
                    var count = Mathf.Min(v.value, 5);
                    for (int i = 0; i < count; i++)
                    {
                        var go = GameObject.Instantiate(prefab);
                        AudioMgr.Instance.PlaySound("Assets/Audios/Drop.mp3", false, go, 1, 6);
                        go.transform.position = pos;
                        Sequence seq = DOTween.Sequence();
                        seq.Append(go.transform.DOMove(pos + new Vector3(Random.Range(-0.5F, 0.5F), Random.Range(0F, 0.5F), Random.Range(-0.5F, 0.5F)), 0.1f));
                        seq.AppendInterval(Random.Range(0.1F, 0.4F));
                        seq.Append(go.transform.DOMove(pos + new Vector3(0, 2, 0), 0.2F));
                        seq.AppendCallback(() =>
                        {
                            _gos.Remove(go);
                            //_sequences.Remove(seq);
                            _assets.Remove(assetID);
                            AudioMgr.Instance.ClearSound(go);
                            AssetsMgr.Instance.Destroy(go);
                        });
                        seq.onComplete = () => { _sequences.Remove(seq); };

                        _gos.Add(go);
                        _sequences.Add(seq);
                    }
                });
                _assets.Add(assetID);
            }
        }
    }
}