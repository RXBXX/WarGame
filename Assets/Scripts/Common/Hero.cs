using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class Hero:Role
    {
        public Hero(int id, RoleAttribute attribute, string prefab, string bornHexagon) : base(id, attribute, prefab, bornHexagon)
        {
            _gameObject.tag = Enum.Tag.Hero.ToString();
            _type = Enum.RoleType.Hero;

        }

        protected override void CreateHUD()
        {
            _hpHUDKey = _id + "_HP";
            HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _gameObject.transform.Find("hudPoint").gameObject, new object[] { _id, 0});
        }
    }
}
