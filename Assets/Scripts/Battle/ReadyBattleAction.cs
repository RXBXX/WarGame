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

        public ReadyBattleAction(int id, int levelID, LevelData data) : base(id, levelID)
        {
            _levelData = data;
            Type = Enum.ActionType.ReadyAction;
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_ShowReady);
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Start, OnReadyOver);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Change_Hero, OnChangeHero);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Start, OnReadyOver);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Change_Hero, OnChangeHero);
        }

        public override void OnClickBegin(GameObject obj)
        {
            var tag = obj.tag;
            if (tag != Enum.Tag.Hero.ToString())
                return;

            _selectedHero = obj.GetComponent<RoleBehaviour>().ID;
            var hexagonID = RoleManager.Instance.GetRole(_selectedHero).Hexagon;
            var hexagon = MapManager.Instance.GetHexagon(hexagonID);
            if ((Enum.HexagonType)hexagon.ConfigID != Enum.HexagonType.Hex22)
                return;

            var screenPos = InputManager.Instance.GetMousePos();
            screenPos.y = Screen.height - screenPos.y;
            var uiPos = GRoot.inst.GlobalToLocal(screenPos);
            var allHeros = DatasMgr.Instance.GetAllRoles();
            var heros = new List<int>();
            foreach (var v in allHeros)
            {
                if (v != _selectedHero)
                {
                    heros.Add(v);
                }
            }

            LockCamera();
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Show_HeroGroup, new object[] { uiPos, heros });
        }

        public override void OnClickEnd()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Hide_HeroGroup);
            UnlockCamera();
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
                    var levelRoleData = new LevelRoleData(roleData.UID, roleData.configId, roleData.level, _selectedHexagon, Enum.RoleState.Waiting, equipDataDic, roleData.talents);
                    levelRoleData.hexagonID = _selectedHexagon;
                    newRole = RoleManager.Instance.CreateRole(Enum.RoleType.Hero, levelRoleData);
                    newRole.GoIntoBattle();
                    newRole.Start();

                    _levelData.heros.Add(levelRoleData);
                }

                if (0 != oldRoleID)
                {
                    var oldRole = RoleManager.Instance.GetRole(oldRoleID);
                    if (newRole.Hexagon != oldRole.Hexagon)
                    {
                        var newRoleHexagon = newRole.Hexagon;
                        newRole.UpdateHexagonID(oldRole.Hexagon, true);
                        newRole.GoIntoBattle();
                        newRole.Start();

                        oldRole.UpdateHexagonID(newRoleHexagon, true);
                        oldRole.GoIntoBattle();
                        oldRole.Start();
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
        }

        private void OnReadyOver(params object[] args)
        {
            AudioMgr.Instance.PlaySound("Assets/Audios/RoundStart.wav");
            _levelData.Stage = Enum.LevelStage.Readyed;

            //������������
            var selectedHeros = new List<int>();
            foreach (var v in _levelData.heros)
                selectedHeros.Add(v.UID);
            DatasMgr.Instance.SetSelectedHeros(selectedHeros);

            OnActionOver(new object[] { 0 });
        }
    }
}