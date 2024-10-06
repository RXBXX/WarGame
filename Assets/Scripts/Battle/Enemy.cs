using System.Collections.Generic;
using WarGame.UI;
using UnityEngine;
using System.Collections;
using FairyGUI;
using DG.Tweening;

namespace WarGame
{
    public class Enemy : Role
    {
        private int _stage;
        //记录当前回合攻击过自己的角色
        private List<int> _attackers = new List<int>();
        private List<int> _findedEnemys = new List<int>();
        private Coroutine _coroutine;
        private bool _isAttacking = false;

        public Enemy(LevelRoleData data) : base(data)
        {
            _type = Enum.RoleType.Enemy;
            _layer = Enum.Layer.Enemy;
            if (GetEnemyConfig().IsBoss)
            {
                _scale = Vector3.one * 0.45f;
            }

            //DebugManager.Instance.Log("Enemy State:" + data.state);
        }

        protected override void OnCreate(GameObject prefab)
        {
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.transform.SetParent(_parent);
            _gameObject.tag = Enum.Tag.Enemy.ToString();

            SmoothNormal();

            _gameObject.transform.position = _position;
            _gameObject.transform.localScale = _scale;

            if (Application.isPlaying)
                UpdateRotation(Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));

            _animator = _gameObject.GetComponent<Animator>();

            _gameObject.GetComponent<RoleBehaviour>().ID = ID; ;

            _hudPoint = _gameObject.transform.Find("hudPoint").gameObject;

            InitEquips();
            InitAnimator();

            if (!Application.isPlaying)
                return;

            CreateHUD();

            OnStateChanged();

            UpdateElementEffects();

            InitBuffEffects();

            EventDispatcher.Instance.PostEvent(Enum.Event.Role_Create_Success, new object[] { ID });
        }

        protected override void CreateHUD()
        {
            base.CreateHUD();
            if (IsBoss())
            {
                GetHUDRole().SetBoss();
            }
            _gameObject.tag = Enum.Tag.Enemy.ToString();
        }

        protected override int GetHPType()
        {
            return 1;
        }

        protected override void OnStateChanged()
        {
            base.OnStateChanged();
            if (_data.state == Enum.RoleState.Waiting)
            {
                _coroutine = CoroutineMgr.Instance.StartCoroutine(StartAI());
            }
        }


        public override void Attack(List<Vector3> hitPoss)
        {
            _isAttacking = true;

            EnterState("Attack");

            foreach (var e in _equipDic)
            {
                e.Value.Attack(hitPoss);
            }
        }

        public override void Hit(float deltaHP, string hitEffect, int attacker)
        {
            if (0 != attacker)
                _attackers.Add(attacker);

            _isAttacking = true;
            if (null != hitEffect)
            {
                AssetsMgr.Instance.LoadAssetAsync<GameObject>(hitEffect, (GameObject prefab) =>
                {
                    var hitPrefab = GameObject.Instantiate(prefab);
                    hitPrefab.transform.position = _gameObject.transform.position + new Vector3(0, 0.8f, 0);
                });
            }

            PlaySound("Assets/Audios/Hit.mp3");
            EnterState("Attacked");

            UpdateAttr(Enum.AttrType.HP, -deltaHP);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetViewDis()
        {
            return GetEnemyConfig().ViewDis;
        }

        protected virtual IEnumerator StartAI()
        {
            //UnityEngine.Profiling.Profiler.BeginSample("StartAI 1111");
            //查找所有视野目标
            var targets = FindingHeros();

            //检测同组队友是否有发现敌人
            var allEnemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);
            for (int i = 0; i < allEnemys.Count; i++)
            {
                var enemy = (Enemy)allEnemys[i];
                if (enemy.ID == ID || !enemy.GetGroup().Equals(GetGroup()))
                    continue;
                var tempTargets = enemy.FindingHeros();
                foreach (var v in tempTargets)
                {
                    if (targets.Contains(v))
                        continue;
                    targets.Add(v);
                }
            }
            //UnityEngine.Profiling.Profiler.EndSample();

            //UnityEngine.Profiling.Profiler.BeginSample("StartAI 2222");
            var moveRegion = MapManager.Instance.FindingMoveRegion(Hexagon, GetMoveDis(), Type);

            var hexagonToRole = new Dictionary<int, int>();
            foreach (var v in RoleManager.Instance.GetAllRoles())
                hexagonToRole.Add(v.Hexagon, v.ID);

            //筛选出在攻击范围内的目标
            Role target = null;
            foreach (var v in targets)
            {
                foreach (var v1 in moveRegion)
                {
                    if (hexagonToRole.ContainsKey(v1.Key) && ID != hexagonToRole[v1.Key])
                        continue;

                    var attachPath = MapManager.Instance.FindingAttackPathForStr(v1.Key, v.Hexagon, GetAttackDis());
                    if (null == attachPath || attachPath.Count <= 0)
                        continue;

                    if ((null == target || target.GetHP() > v.GetHP()))
                    //var pos = GetPosition();
                    //if(null == target || Vector3.Distance(target.GetPosition(), pos) > Vector3.Distance(v.GetPosition(), pos))
                    {
                        target = v;
                        break;
                    }
                }
            }
            //UnityEngine.Profiling.Profiler.EndSample();

            //UnityEngine.Profiling.Profiler.BeginSample("StartAI 3333");

            bool showWarning = false;
            List<int> path = null;
            if (null != target)
            {
                //计算处移动代价最小的路径选择
                Cell destCell = null;
                foreach (var v in moveRegion)
                {
                    if (hexagonToRole.ContainsKey(v.Key) && ID != hexagonToRole[v.Key])
                        continue;

                    var attachPath = MapManager.Instance.FindingAttackPathForStr(v.Key, target.Hexagon, GetAttackDis());
                    if (null != attachPath && attachPath.Count > 0 && (null == destCell || destCell.g > v.Value.g))
                    {
                        //DebugManager.Instance.Log(v.Key);
                        destCell = v.Value;
                    }
                }
                if (null != destCell)
                {
                    path = new List<int>();
                    while (null != destCell)
                    {
                        path.Insert(0, destCell.id);

                        destCell = destCell.parent;
                    }
                }

                showWarning = true;
                //SetHPVisible(true);
            }
            else if (targets.Count > 0)
            {
                //找出最近的敌人，并向其靠近
                Role hero = targets[0];
                foreach (var v in targets)
                {
                    if (hero.GetHP() > v.GetHP())
                    {
                        hero = v;
                    }
                }

                var targetHexagon = MapManager.Instance.GetHexagon(hero.Hexagon);
                Cell destCell = null;
                foreach (var v in moveRegion)
                {
                    if (hexagonToRole.ContainsKey(v.Key) && ID != hexagonToRole[v.Key])
                        continue;

                    if (null == destCell || WGVector3.Distance(targetHexagon.coor, destCell.coor) > WGVector3.Distance(targetHexagon.coor, v.Value.coor))
                        destCell = v.Value;
                }
                //DebugManager.Instance.Log(destCell.id);
                if (null != destCell)
                {
                    path = new List<int>();
                    while (null != destCell)
                    {
                        path.Insert(0, destCell.id);
                        destCell = destCell.parent;
                    }
                    //DebugManager.Instance.Log("Path:"+path.Count);
                }
                //DebugManager.Instance.Log(path.Count);

                //SetHPVisible(true);
                showWarning = true;
            }
            else if (Hexagon != _data.bornHexagonID)
            {
                var movePath = MapManager.Instance.FindingPath(Hexagon, _data.bornHexagonID, Type);
                if (null != movePath)
                {
                    if (movePath.Count > 0)
                    {
                        for (int i = movePath.Count - 1; i > 0; i--)
                        {
                            if (movePath[i].g > GetMoveDis())
                            {
                                movePath.RemoveAt(i);
                                continue;
                            }
                            break;
                        }

                        path = new List<int>();
                        for (int i = 0; i < movePath.Count; i++)
                            path.Add(movePath[i].id);
                    }
                    movePath.Recycle();
                }
            }
            else if (WillAct())
            {
                //在出生点和随机点之间移动
                var emptyMoveRegions = MapManager.Instance.PopCellListStack();
                foreach (var v in moveRegion)
                {
                    if (hexagonToRole.ContainsKey(v.Key) && ID != hexagonToRole[v.Key])
                        continue;

                    emptyMoveRegions.Add(v.Value);
                }
                if (emptyMoveRegions.Count > 0)
                {
                    var randomHexagonIndex = Random.Range(0, emptyMoveRegions.Count);
                    var rdHexagon = emptyMoveRegions[randomHexagonIndex];
                    if (rdHexagon.id != Hexagon)
                    {
                        path = new List<int>();
                        while (null != rdHexagon)
                        {
                            path.Insert(0, rdHexagon.id);
                            rdHexagon = rdHexagon.parent;
                        }
                    }
                }
                emptyMoveRegions.Recycle();
            }

            //UnityEngine.Profiling.Profiler.EndSample();

            moveRegion.Recycle();


            var skillID = SelectSkill();
            var move = null != path && path.Count > 1;
            //DebugManager.Instance.Log("StartAI:" + ID + "_" +( null == target ? 0 : target.ID));
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AI_Start, new object[] { ID, null == target ? 0 : target.ID, skillID, move });
            //yield return new WaitForSeconds(1.0F);
            //yield return null;

            if (showWarning)
            {
                yield return new WaitForSeconds(GetHUDRole().Warning());
            }

            //DebugManager.Instance.Log(path.Count);
            if (move)
            {
                Move(path);
                //DebugManager.Instance.Log("Move:" + ID);
            }
            else if (null != target)
            {
                MoveEnd();
            }
            else
            {
                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AI_Over, new object[] { 0 });
            }

            _coroutine = null;
        }

        public override void Move(List<int> hexagons)
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AI_MoveStart, new object[] { ID });
            base.Move(hexagons);
        }

        public override void ResetState()
        {
            SetState(Enum.RoleState.Locked);
        }

        public override void UpdateRound(Enum.RoleType type)
        {
            UpdateBuffs(type);

            if (type != Type)
                return;

            if (!IsSilence() && _isAttacking)
                UpdateAttr(Enum.AttrType.Rage, GetAttribute(Enum.AttrType.RageRecover));

            if (!_isAttacking)
            {
                UpdateAttr(Enum.AttrType.HP, GetAttribute(Enum.AttrType.HPRecover));
            }

            ResetState();


            if (type != Type)
                return;

            _attackers.Clear();
            _findedEnemys.Clear();

            _isAttacking = false;
        }

        protected override int GetNextStage()
        {
            if (_stage > 0)
                return 0;

            return ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", ID).NextStage;
        }

        public override bool HaveNextStage()
        {
            return 0 != GetNextStage();
        }

        public override void NextStage()
        {
            var UID = _data.UID;
            var hexagon = _data.hexagonID;

            base.Dispose();

            _data = Factory.Instance.GetLevelRoleData(Enum.RoleType.Enemy, GetNextStage(), hexagon);
            _data.UID = UID;
            _stage++;
            DeadFlag = false;

            CreateGO();
        }

        //查找所有视野目标
        public List<Role> FindingHeros()
        {
            if (_findedEnemys.Count <= 0)
            {
                foreach (var v in _attackers)
                {
                    var role = RoleManager.Instance.GetRole(v);
                    if (null == role)
                        continue;

                    if (!role.Visible())
                        continue;

                    _findedEnemys.Add(role.ID);
                }

                var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
                var viewRegionDic = MapManager.Instance.FindingViewRegion(Hexagon, GetViewDis());
                //DebugManager.Instance.Log("FindingViewRegion:" + count);
                foreach (var v in heros)
                {
                    if (_findedEnemys.Contains(v.ID))
                        continue;

                    if (!v.Visible())
                        continue;

                    if (!viewRegionDic.ContainsKey(v.Hexagon))
                        continue;
                    _findedEnemys.Add(v.ID);
                }

                viewRegionDic.Recycle();
            }

            var targets = new List<Role>() { };
            foreach (var v in _findedEnemys)
            {
                var target = RoleManager.Instance.GetRole(v);
                if (null != target)
                    targets.Add(target);
            }

            return targets;
        }

        public EnemyConfig GetEnemyConfig()
        {
            return ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", _data.UID);
        }

        public string GetGroup()
        {
            return GetEnemyConfig().Group;
        }

        protected override void SetVisible(bool visible)
        {
            SetColliderEnable(visible);
            HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey).SetVisible(visible);
            Tool.Instance.SetAlpha(_gameObject.gameObject, visible ? 1 : 0);
        }

        public override bool CanAction()
        {
            if (GetState() != Enum.RoleState.Locked)
                return false;

            //foreach (var v in _data.buffs)
            //    DebugManager.Instance.Log(v.id);

            return base.CanAction();
        }

        public override void ShowDrops()
        {
            var reward = GetEnemyConfig().Reward;
            if (0 == reward)
                return;

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Drops, new object[] { reward, GetPosition() });
        }

        private int SelectSkill()
        {
            if (GetRage() >= GetAttribute(Enum.AttrType.Rage))
            {
                if (GetConfig().SpecialSkill == (int)Enum.Skill.CriticalHit)
                    return GetConfig().SpecialSkill;
            }
            return GetConfig().CommonSkill;
        }

        private bool IsBoss()
        {
            return GetEnemyConfig().IsBoss;
        }

        private bool WillAct()
        {
            return Random.Range(0, 10) < GetEnemyConfig().ActPreference;
        }

        public override bool Dispose()
        {
            if (null != _coroutine)
            {
                CoroutineMgr.Instance.StopCoroutine(_coroutine);
                _coroutine = null;
            }
            return base.Dispose();
        }
    }
}
