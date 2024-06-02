using System.Collections.Generic;
using WarGame.UI;
using UnityEngine;
using System.Collections;

namespace WarGame
{
    public class Enemy : Role
    {
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
            _hpHUDKey = _id + "_HP";
            var hud = (HUDRole)HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _hudPoint, new object[] { _id, 1 });
            hud.Init(GetHP(), GetAttribute(Enum.AttrType.HP), GetRage(), GetAttribute(Enum.AttrType.Rage));
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
            List<string> path = null;
            for (int i = 0; i < heros.Count; i++)
            {
                DebugManager.Instance.Log("Target:" + heros[i].Hexagon);
                var tempPath = MapManager.Instance.FindingAIPath(Hexagon, heros[i].Hexagon, GetMoveDis(), GetAttackDis());
                if (null!= tempPath && tempPath.Count > 0)
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

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Start, new object[] { _id, targetID, GetConfig().CommonSkill});
            yield return new WaitForSeconds(1.0F);
            if (targetID <= 0)
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Over, new object[] { _id, targetID, GetConfig().CommonSkill });
            }
            else
            {
                if (null != path)
                {
                    Move(path);
                }
                else
                {
                    MoveEnd();
                }
            }
        }

        public override void Move(List<string> hexagons)
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_MoveStart, new object[] { _id });
            base.Move(hexagons);
        }

        public override void UpdateRound()
        {
            base.UpdateRound();
            SetState(Enum.RoleState.Locked);
        }
    }
}
