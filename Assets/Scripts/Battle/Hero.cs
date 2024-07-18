using WarGame.UI;
using UnityEngine;
using FairyGUI;
using System.Collections.Generic;

namespace WarGame
{
    public class Hero:Role
    {
        private ParticleSystem _smoke;

        public Hero(LevelRoleData data) : base(data)
        {
            _type = Enum.RoleType.Hero;
            _layer = Enum.Layer.Hero;
        }

        protected override void OnCreate(GameObject go)
        {
            base.OnCreate(go);
            _gameObject.tag = Enum.Tag.Hero.ToString();

            var smoke = _gameObject.transform.Find("root/pelvis/smoke");
            if (null != smoke)
            {
                smoke.gameObject.SetActive(true);
                _smoke = smoke.GetComponent<ParticleSystem>();
                StopSmoke();
            }
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

        protected override void SetVisible(bool visible)
        {
            DebugManager.Instance.Log("SetVisible:"+visible);
            Tool.Instance.SetAlpha(_gameObject.gameObject, visible ? 1 : 0.3F);
        }

        public override LevelRoleData Clone(int hexagon, int cloneUID)
        {
            var data = _data.Clone();
            data.UID = cloneUID;
            var heroConfig = ConfigMgr.Instance.GetConfig<HeroConfig>("HeroConfig", cloneUID);
            data.configId = heroConfig.RoleID;
            data.level = heroConfig.Level;
            data.hexagonID = hexagon;
            data.bornHexagonID = hexagon;

            _data.cloneRole = data.UID;

            data.state = Enum.RoleState.Waiting;
            return data;
        }

        public override bool CanAction()
        {
            if (GetState() != Enum.RoleState.Waiting)
                return false;

            return base.CanAction();
        }

        public override void Move(List<int> hexagons)
        {
            PlaySmoke();
            base.Move(hexagons);
        }

        public override void MoveEnd()
        {
            StopSmoke();
            base.MoveEnd();
        }

        private void PlaySmoke()
        {
            if (null == _smoke)
                return;
            var emisson = _smoke.emission;
            emisson.enabled = true;
        }

        private void StopSmoke()
        {
            if (null == _smoke)
                return;
            var emisson = _smoke.emission;
            emisson.enabled = false;
        }
    }
}
