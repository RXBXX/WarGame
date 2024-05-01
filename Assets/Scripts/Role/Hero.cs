using WarGame.UI;

namespace WarGame
{
    public class Hero:Role
    {
        public Hero(RoleData data, string bornHexagon) : base(data, bornHexagon)
        {
            _gameObject.tag = Enum.Tag.Hero.ToString();
            _type = Enum.RoleType.Hero;
            _layer = 6;
        }

        protected override void CreateHUD()
        {
            _hpHUDKey = _id + "_HP";
            HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _hudPoint, new object[] { _id, 0});
        }

        public override Enum.RoleType GetTargetType()
        {
            return Enum.RoleType.Enemy;
        }

        public override void MoveEnd()
        {
            base.MoveEnd();
            EventDispatcher.Instance.PostEvent(Enum.EventType.Role_MoveEnd_Event);
        }

        protected override void OnStateChanged()
        {
            base.OnStateChanged();
            if (_state == Enum.RoleState.WatingTarget)
            {
                var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
                hud.SetFightCancelVisible(true);
            }
            else if (_state == Enum.RoleState.WaitingOrder)
            {
                var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
                hud.SetFightCancelVisible(false);
            }
        }
    }
}
