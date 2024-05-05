using WarGame.UI;

namespace WarGame
{
    public class Enemy : Role
    {
        private int _target;

        public Enemy(RoleData data, string bornHexagon) : base(data, bornHexagon)
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
            if (_state == Enum.RoleState.Waiting)
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_AI_Start, new object[] { _id });
                StartAI();
            }
            else if (_state == Enum.RoleState.Over)
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Attack_End, new object[] { _id });
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
                        SetState(Enum.RoleState.Moving);
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
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Attack, new object[] { _id, _target });
                _target = 0;
            }
            else
            {
                SetState(Enum.RoleState.Over);
                Idle();
            }
        }
    }
}
