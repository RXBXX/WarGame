using UnityEngine;
using FairyGUI;
using System.Collections;
using System.Collections.Generic;

namespace WarGame
{
    public class ReadyBattleAction : BattleAction
    {
        //private int _touchingID = 0;
        private int _selectedHero = 0;
        private LevelData _levelData;

        public ReadyBattleAction(LevelData data) : base()
        {
            _levelData = data;
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Change_Hero, OnChangeHero);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Change_Hero, OnChangeHero);
        }

        public override void OnClickBegin(GameObject obj)
        {
            var tag = obj.tag;
            if (tag == Enum.Tag.Hero.ToString())
            {
                _selectedHero = obj.GetComponent<RoleBehaviour>().ID;

                var hexagonID = RoleManager.Instance.GetRole(_selectedHero).Hexagon;
                var hexagon = MapManager.Instance.GetHexagon(hexagonID);
                if ((Enum.HexagonType)hexagon.ConfigID != Enum.HexagonType.Born)
                    return;

                var screenPos = InputManager.Instance.GetMousePos();
                screenPos.y = Screen.height - screenPos.y;
                var uiPos = GRoot.inst.GlobalToLocal(screenPos);
                var allHeros = DatasMgr.Instance.GetAllRoles();
                var heros = new int[allHeros.Length - 1];
                var index = 0;
                foreach (var v in allHeros)
                {
                    if (v != _selectedHero)
                    {
                        heros[index] = v;
                        index += 1;
                    }
                }
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Show_HeroGroup, new object[] { uiPos, heros });
            }
        }

        protected override void OnActionOver(params object[] args)
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Action_Over);
        }

        private void OnChangeHero(params object[] args)
        {
            var oldRoleID = _selectedHero;
            var _selectedHexagon = RoleManager.Instance.GetRole(oldRoleID).Hexagon;
            if (null == args[0] || 0 == (int)args[0])
            {
                if (0 != oldRoleID)
                {
                    RoleManager.Instance.RemoveRole(oldRoleID);
                    foreach (var v in _levelData.heros)
                    {
                        if (v.UID == oldRoleID)
                        {
                            _levelData.heros.Remove(v);
                            break;
                        }
                    }
                }
            }
            else
            {
                var newRole = RoleManager.Instance.GetRole((int)args[0]);
                if (null == newRole)
                {
                    var roleData = DatasMgr.Instance.GetRoleData((int)args[0]);
                    var equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                    foreach (var v in roleData.equipmentDic)
                    {
                        equipDataDic.Add(v.Key, DatasMgr.Instance.GetEquipmentData(v.Value));
                    }
                    var levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, Enum.RoleState.Waiting, equipDataDic, roleData.talentDic);
                    levelRoleData.hp = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + roleData.level).HP;
                    levelRoleData.hexagonID = _selectedHexagon;
                    newRole = RoleManager.Instance.CreateHero(levelRoleData);

                    _levelData.heros.Add(levelRoleData);
                }

                if (0 != oldRoleID)
                {
                    var oldRole = RoleManager.Instance.GetRole(oldRoleID);
                    if (newRole.Hexagon != oldRole.Hexagon)
                    {
                        var newRoleHexagon = newRole.Hexagon;
                        newRole.UpdateHexagonID(oldRole.Hexagon);
                        oldRole.UpdateHexagonID(newRoleHexagon);
                    }
                    else
                    {
                        RoleManager.Instance.RemoveRole(oldRoleID);
                        foreach (var v in _levelData.heros)
                        {
                            if (v.UID == oldRoleID)
                            {
                                _levelData.heros.Remove(v);
                                break;
                            }
                        }
                    }
                }
            }

            _selectedHexagon = null;
        }
    }
}