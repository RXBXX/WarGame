using System.Collections.Generic;
using WarGame.UI;

namespace WarGame
{
    public class Enemy : Role
    {
        public Enemy(LevelRoleData data) : base(data)
        {
            _gameObject.tag = Enum.Tag.Enemy.ToString();
            _type = Enum.RoleType.Enemy;
            _layer = 7;
        }

        protected override void CreateHUD()
        {
            _hpHUDKey = _id + "_HP";
            HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _hudPoint, new object[] { _id, 1 });
        }

        protected override void OnStateChanged()
        {
            base.OnStateChanged();
            if (_state == Enum.RoleState.Waiting)
            {
                StartAI();
            }
        }

        protected virtual void StartAI()
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            var targetID = 0;
            List<string> path = null;
            for (int i = 0; i < heros.Count; i++)
            {
                var tempPath = MapManager.Instance.FindingAIPath(hexagonID, heros[i].hexagonID, GetMoveDis(), GetAttackDis());
                if (null!= tempPath && tempPath.Count > 0)
                {
                    if (hexagonID == tempPath[tempPath.Count - 1])
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
            if (targetID <= 0)
            {
                SetState(Enum.RoleState.Over);
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

        protected override void InitEquips()
        {
            if (null == _data.equipmentDic)
                return;

            foreach (var v in _data.equipmentDic)
            {
                var equipPlaceConfig = ConfigMgr.Instance.GetConfig<EquipPlaceConfig>("EquipPlaceConfig", (int)v.Key);
                var spinePoint = _gameObject.transform.Find(equipPlaceConfig.SpinePoint);

                var equip = EquipFactory.Instance.GetEquip(new EquipmentData(0, v.Value));
                equip.SetSpinePoint(spinePoint);
                _equipDic[equip.GetPlace()] = equip;
            }
        }
    }
}
