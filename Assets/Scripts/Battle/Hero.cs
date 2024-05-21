using WarGame.UI;
using UnityEngine;

namespace WarGame
{
    public class Hero:Role
    {
        public Hero(LevelRoleData data) : base(data)
        {
            _type = Enum.RoleType.Hero;
            _layer = 6;
        }

        protected override void OnCreate(GameObject go)
        {
            base.OnCreate(go);
            _gameObject.tag = Enum.Tag.Hero.ToString();
        }

        protected override void CreateHUD()
        {
            _hpHUDKey = _id + "_HP";
            var hud = (HUDRole)HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _hudPoint, new object[] { _id, 0 });
            hud.UpdateHP(_data.GetAttribute(Enum.AttrType.HP));
        }

        public override void UpdateRound()
        {
            base.UpdateRound();
            SetState(Enum.RoleState.Waiting);
        }
    }
}
