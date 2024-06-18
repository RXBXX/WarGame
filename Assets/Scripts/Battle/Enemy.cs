using System.Collections.Generic;
using WarGame.UI;
using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class Enemy : Role
    {
        private int _stage;

        public Enemy(LevelRoleData data) : base(data)
        {
            _type = Enum.RoleType.Enemy;
            _layer = Enum.Layer.Enemy;
        }

        protected override void OnCreate(GameObject go)
        {
            base.OnCreate(go);
            _gameObject.tag = Enum.Tag.Enemy.ToString();
        }

        protected override void CreateHUD()
        {
            _hpHUDKey = ID + "_HP";
            var args = new object[] { ID, 1, GetHP(), GetAttribute(Enum.AttrType.HP), GetRage(), GetAttribute(Enum.AttrType.Rage), GetElement() };
            HUDManager.Instance.AddHUD<HUDRole>("HUD", "HUDRole", _hpHUDKey, _hudPoint, args);
        }

        protected override void OnStateChanged()
        {
            base.OnStateChanged();
            if (_data.state == Enum.RoleState.Waiting)
            {
                CoroutineMgr.Instance.StartCoroutine(StartAI());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetViewDis()
        {
            return ConfigMgr.Instance.GetConfig<EnemyConfig>("LevelEnemyConfig", _data.UID).ViewDis;
        }

        protected virtual IEnumerator StartAI()
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            var moveRegion = MapManager.Instance.FindingMoveRegion(Hexagon, GetMoveDis(), Type);

            //查找所有视野目标
            //将最后一次攻击自己的敌人也加入队列
            var targets = new List<Role>() { };
            if (0 != _attacker)
            {
                var attacker = RoleManager.Instance.GetRole(_attacker);
                targets.Add(attacker);
            }

            var viewRegionDic = MapManager.Instance.FindingViewRegion(Hexagon, GetViewDis());
            //DebugManager.Instance.Log("FindingView"+viewRegionDic.Count);

            //foreach (var v in viewRegionDic)
            //    DebugManager.Instance.Log(v.Key);
            foreach (var v in heros)
            {
                if (!viewRegionDic.ContainsKey(v.Hexagon))
                    continue;
                //DebugManager.Instance.Log(v.ID);
                targets.Add(v);
            }

            //DebugManager.Instance.Log("FilterTarget");
            //筛选出攻击目标
            Role target = null;
            foreach (var v in targets)
            {
                foreach (var v1 in moveRegion)
                {
                    var hexagonRoleID = RoleManager.Instance.GetRoleIDByHexagonID(v1.Key);
                    if (0 != hexagonRoleID && ID != hexagonRoleID)
                        continue;

                    var attachPath = MapManager.Instance.FindingAttackPathForStr(v1.Key, v.Hexagon, GetAttackDis());
                    if (null != attachPath && attachPath.Count > 0 && (null == target || target.GetHP() > v.GetHP()))
                    {
                        DebugManager.Instance.Log(v.ID);
                        target = v;
                        break;
                    }
                }
            }

            List<string> path = null;
            if (null != target)
            {
                MapManager.Cell destCell = null;
                foreach (var v in moveRegion)
                {
                    var hexagonRoleID = RoleManager.Instance.GetRoleIDByHexagonID(v.Key);
                    if (0 != hexagonRoleID && hexagonRoleID != ID)
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
                    path = new List<string>();
                    while (null != destCell)
                    {
                        path.Insert(0, destCell.id);
                        destCell = destCell.parent;
                    }
                }
            }
            else if (targets.Count > 0)
            {
                //DebugManager.Instance.Log("Targets.Count > 0");
                Role hero = targets[0];
                foreach (var v in targets)
                {
                    if (hero.GetHP() > v.GetHP())
                    {
                        hero = v;
                    }
                }

                var targetHexagon = MapManager.Instance.GetHexagon(hero.Hexagon);
                MapManager.Cell destCell = null;
                foreach (var v in moveRegion)
                {
                    var hexagonRoleID = RoleManager.Instance.GetRoleIDByHexagonID(v.Key);
                    if (0 != hexagonRoleID && hexagonRoleID != ID)
                        continue;

                    if (null == destCell || Vector3.Distance(targetHexagon.coor, destCell.coor) > Vector3.Distance(targetHexagon.coor, v.Value.coor))
                        destCell = v.Value;
                }
                if (null != destCell)
                {
                    path = new List<string>();
                    while (null != destCell)
                    {
                        path.Insert(0, destCell.id);
                        destCell = destCell.parent;
                    }
                    //DebugManager.Instance.Log("Path:"+path.Count);
                }
            }
            else
            {
                if (Hexagon == _data.bornHexagonID)
                {
                    var emptyMoveRegions = new List<MapManager.Cell>();
                    foreach (var v in moveRegion)
                    {
                        var hexagonRoleID = RoleManager.Instance.GetRoleIDByHexagonID(v.Key);
                        if (0 != hexagonRoleID && hexagonRoleID != ID)
                            continue;
                        emptyMoveRegions.Add(v.Value);
                    }
                    var randomHexagonIndex = Random.Range(0, emptyMoveRegions.Count);
                    var rdHexagon = emptyMoveRegions[randomHexagonIndex];
                    if (rdHexagon.id != Hexagon)
                    {
                        path = new List<string>();
                        while (null != rdHexagon)
                        {
                            path.Insert(0, rdHexagon.id);
                            rdHexagon = rdHexagon.parent;
                        }
                    }
                }
                else
                {
                    var movePath = MapManager.Instance.FindingPath(Hexagon, _data.bornHexagonID, Type);
                    if (null != movePath && movePath.Count > 0)
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

                        path = new List<string>();
                        for (int i = 0; i < movePath.Count; i++)
                            path.Add(movePath[i].id);
                    }
                }
            }

            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AI_Start, new object[] { ID, null == target?0:target.ID, GetConfig().CommonSkill });
            yield return new WaitForSeconds(1.0F);

            //DebugManager.Instance.Log(path.Count);
            if (null!= path && path.Count > 1)
                Move(path);
            else if (null != target)
                MoveEnd();
            else
                EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AI_Over, new object[] { 0 });
        }

        public override void Move(List<string> hexagons)
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_AI_MoveStart, new object[] { ID });
            base.Move(hexagons);
        }

        public override void UpdateRound()
        {
            base.UpdateRound();
            SetState(Enum.RoleState.Locked);
            _attacker = 0;
        }

        protected override int GetNextStage()
        {
            if (_stage > 0)
                return 0;

            return ConfigMgr.Instance.GetConfig<EnemyConfig>("LevelEnemyConfig", ID).NextStage;
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

            _data = DatasMgr.Instance.CreateLevelRoleData(Enum.RoleType.Enemy, GetNextStage(), hexagon);
            _data.UID = UID;
            _stage++;
            DeadFlag = false;

            CreateGO();
        }
    }
}
