using WarGame.UI;

namespace WarGame
{
    public class Enemy : Role
    {
        private int _target;

        public Enemy(int id, RoleAttribute attribute, string prefab, string bornHexagon) : base(id, attribute, prefab, bornHexagon)
        {
            _gameObject.tag = Enum.Tag.Enemy.ToString();
            _type = Enum.RoleType.Enemy;
        }

        protected override void CreateHUD()
        {
            _hpHUDKey = _id + "_HP";
            HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _gameObject.transform.Find("hudPoint").gameObject, new object[] { _id, 1 });
        }

        public override void OnStateChanged()
        {
            if (_state == Enum.RoleState.Attacking)
                StartAI();
        }

        public void StartAI()
        {
            var heros = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Hero);
            for (int i = 0; i < heros.Count; i++)
            {
                var path = MapManager.Instance.FindingAIPath(hexagonID, heros[i].hexagonID, GetMoveDis(), GetAttackDis());
                if (null != path && path.Count > 0)
                {
                    if (hexagonID == path[path.Count - 1])
                    {
                        _target = heros[i].ID;
                        MoveEnd();
                    }
                    else if (RoleManager.Instance.GetRoleIDByHexagonID(path[path.Count - 1]) > 0)
                    {
                        RoleManager.Instance.NextState(_id);
                    }
                    else
                    {
                        _target = heros[i].ID;
                        Move(path);
                    }
                    return;
                }
            }

            RoleManager.Instance.NextState(_id);
        }

        public override void MoveEnd()
        {
            if (_target > 0)
            {
                RoleManager.Instance.Attack(_id, _target);
                _target = 0;
            }
            else
            {
                Idle();
                RoleManager.Instance.NextState(_id);
            }
        }
    }
}
