using WarGame.UI;
using UnityEngine;
using FairyGUI;

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
            base.CreateHUD();
            _hpHUDKey = ID + "_HP";
            var args = new object[] { ID, 0, GetHP(), GetAttribute(Enum.AttrType.HP), GetRage(), GetAttribute(Enum.AttrType.Rage), GetElement()};
            var hud = HUDManager.Instance.AddHUD<HUDRole>("HUDRole", _hpHUDKey, _hudPoint.GetComponent<UIPanel>().ui, _hudPoint, args);
            hud.SetHPVisible(true);
        }

        public override void ResetState()
        {
            SetState(Enum.RoleState.Waiting);
        }

        public override void UpdateRound(Enum.RoleType type)
        {
            base.UpdateRound(type);
            if (type != Type)
                return;
            UpdateAttr(Enum.AttrType.Rage, GetAttribute(Enum.AttrType.RageRecover));
            ResetState();
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

        public override bool CanAction()
        {
            if (GetState() != Enum.RoleState.Waiting)
                return false;

            return base.CanAction();
        }
    }
}
