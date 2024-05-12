using WarGame.UI;

namespace WarGame
{
    public class Enemy : Role
    {
        private int _target;

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
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Start, new object[] { _id });
                StartAI();
            }
        }

        public void StartAI()
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            for (int i = 0; i < heros.Count; i++)
            {
                var path = MapManager.Instance.FindingAIPath(hexagonID, heros[i].hexagonID, GetMoveDis(), GetAttackDis());
                if (null!= path && path.Count > 0)
                {
                    if (hexagonID == path[path.Count - 1])
                    {
                        _target = heros[i].ID;
                        MoveEnd();
                        return;
                    }
                    else if (RoleManager.Instance.GetRoleIDByHexagonID(path[path.Count - 1]) > 0)
                    {
                        //SetState(Enum.RoleState.Over);
                    }
                    else
                    {
                        _target = heros[i].ID;
                        EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Move, new object[] { _id });
                        Move(path);
                        return;
                    }
                }
            }
            SetState(Enum.RoleState.Over);
        }

        public override void MoveEnd()
        {
            base.MoveEnd();
            if (_target > 0)
            {
                SetState(Enum.RoleState.Attacking);
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Attack, new object[] { _id, _target, Enum.SkillType.Common});
                _target = 0;
            }
            else
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Over, new object[] { _id });
                Idle();
            }
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
