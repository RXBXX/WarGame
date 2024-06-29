using WarGame.UI;
using UnityEngine;

namespace WarGame
{
    public class Hero:Role
    {
        public Hero(LevelRoleData data) : base(data)
        {
            _type = Enum.RoleType.Hero;
            _layer = Enum.Layer.Hero;
        }

        protected override void OnCreate(GameObject go)
        {
            base.OnCreate(go);
            _gameObject.tag = Enum.Tag.Hero.ToString();
        }

        protected override void CreateHUD()
        {
            _hpHUDKey = ID + "_HP";
            var args = new object[] { ID, 0, GetHP(), GetAttribute(Enum.AttrType.HP), GetRage(), GetAttribute(Enum.AttrType.Rage), GetElement() };
            HUDManager.Instance.AddHUD<HUDRole>("HUD", "HUDRole", _hpHUDKey, _hudPoint, args);
            //hud.Init(GetHP(), GetAttribute(Enum.AttrType.HP), GetRage(), GetAttribute(Enum.AttrType.Rage));
        }

        public override void UpdateRound()
        {
            base.UpdateRound();
            SetState(Enum.RoleState.Waiting);
        }

        protected override void SetVisible(bool visible)
        {
            DebugManager.Instance.Log("SetVisible:"+visible);
            Tool.Instance.SetAlpha(_gameObject.gameObject, visible ? 1 : 0.3F);
        }

        public override LevelRoleData Clone(string hexagon)
        {
            var data = base.Clone(hexagon);
            data.state = Enum.RoleState.Waiting;
            return data;
        }
    }
}
