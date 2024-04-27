using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class Hero:Role
    {
        public Hero(int id, int configId, string bornHexagon) : base(id, configId, bornHexagon)
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
    }
}
