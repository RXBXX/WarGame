using WarGame.UI;

namespace WarGame
{
    public class Hero:Role
    {
        public Hero(LevelRoleData data) : base(data)
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

        public override void MoveEnd()
        {
            base.MoveEnd();
            EventDispatcher.Instance.PostEvent(Enum.EventType.Role_MoveEnd_Event);
        }

        public override void UpdateRound()
        {
            base.UpdateRound();
            SetState(Enum.RoleState.Waiting);
        }

        protected override void InitEquips()
        {
            if (null == _data.equipmentDic)
                return;

            foreach (var v in _data.equipmentDic)
            {
                var equipPlaceConfig = ConfigMgr.Instance.GetConfig<EquipPlaceConfig>("EquipPlaceConfig", (int)v.Key);
                var spinePoint = _gameObject.transform.Find(equipPlaceConfig.SpinePoint);

                var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                var equip = EquipFactory.Instance.GetEquip(equipData);
                equip.SetSpinePoint(spinePoint);
                _equipDic[equip.GetPlace()] = equip;
            }
        }
    }
}
