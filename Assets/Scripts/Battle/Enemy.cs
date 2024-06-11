using System.Collections.Generic;
using WarGame.UI;
using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class Enemy : Role
    {
        private int _stage;
        private string _bornHexagon;

        public Enemy(LevelRoleData data) : base(data)
        {
            _type = Enum.RoleType.Enemy;
            _layer = Enum.Layer.Enemy;
            _bornHexagon = Hexagon;
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

        protected virtual IEnumerator StartAI()
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            var targetID = 0;
            List<string> path = new List<string>();

            for (int i = 0; i < heros.Count; i++)
            {
                var tempPath = MapManager.Instance.FindingAIPath(Hexagon, heros[i].Hexagon, GetMoveDis(), GetAttackDis());
                if (null != tempPath && tempPath.Count > 0)
                {
                    if (Hexagon == tempPath[tempPath.Count - 1])
                    {
                        targetID = heros[i].ID;
                        break;
                    }
                    else if (RoleManager.Instance.GetRoleIDByHexagonID(tempPath[tempPath.Count - 1]) > 0)
                    {
                        //SetState(Enum.RoleState.Over);
                    }
                    else
                    {
                        path = tempPath;
                        targetID = heros[i].ID;
                        break;
                    }
                }
            }

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Start, new object[] { ID, targetID, GetConfig().CommonSkill });
            yield return new WaitForSeconds(1.0F);

            if (targetID <= 0)
            {
                //DebugManager.Instance.Log("MoveDis:" + GetMoveDis());
                var rdMoveDis = Random.Range(0, GetMoveDis());
                //DebugManager.Instance.Log("RandomMoveDis:" + rdMoveDis);
                if (Hexagon == _bornHexagon)
                {
                    var moveRegion = MapManager.Instance.FindingMoveRegion(Hexagon, rdMoveDis, Type);
                    var randomHexagonIndex = Random.Range(0, moveRegion.Count);
                    //DebugManager.Instance.Log("RandomHexagon:" + randomHexagonIndex);
                    var rdHexagon = moveRegion[randomHexagonIndex];
                    //DebugManager.Instance.Log("RandomTargetHexagon:" + rdHexagon.id);
                    if (rdHexagon.id != Hexagon)
                    {
                        while (null != rdHexagon)
                        {
                            path.Insert(0, rdHexagon.id);
                            rdHexagon = rdHexagon.parent;
                        }
                    }
                }
                else
                {
                    var movePath = MapManager.Instance.FindingPath(Hexagon, _bornHexagon, Type);
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
                    }

                    for (int i = 0; i < movePath.Count; i++)
                        path.Add(movePath[i].id);
                }
            }

            //DebugManager.Instance.Log(path.Count);
            if (path.Count > 0)
                Move(path);
            else if (targetID > 0)
                MoveEnd();
            else
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Over, new object[] { 0 });
        }

        public override void Move(List<string> hexagons)
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_MoveStart, new object[] { ID });
            //foreach (var v in hexagons)
            //    DebugManager.Instance.Log(v);
            base.Move(hexagons);
        }

        public override void UpdateRound()
        {
            base.UpdateRound();
            SetState(Enum.RoleState.Locked);
        }

        protected override int GetNextStage()
        {
            if (_stage > 0)
                return 0;

            return ConfigMgr.Instance.GetConfig<LevelEnemyConfig>("LevelEnemyConfig", ID).NextStage;
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

            _data = DatasMgr.Instance.CreateLevelRoleData(Enum.RoleType.Enemy, GetNextStage());
            _data.UID = UID;
            _data.hexagonID = hexagon;
            _stage++;
            DeadFlag = false;

            CreateGO();
        }
    }
}
