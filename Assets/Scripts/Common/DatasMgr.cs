using System.Collections.Generic;
using UnityEngine;
using System;

namespace WarGame
{
    public class DatasMgr : Singeton<DatasMgr>
    {
        private GameData _data = null;
        private string _path = Application.dataPath + "/Datas/GameData.json";

        public override bool Init()
        {
            base.Init();

            _data = Tool.Instance.ReadJson<GameData>(_path);
            if (null == _data)
                _data = new GameData();

            return true;
        }


        public override bool Dispose()
        {
            _data.Save();
            Tool.Instance.WriteJson<GameData>(_path, _data);
            return base.Dispose();
        }

        public void StartNewGame()
        {
            _data.StartNewGame();
        }

        public void StartGame(string id)
        {
            _data.Start(id);
        }

        public void SaveGame(string id = null)
        {
            _data.SaveRecord(id);
        }

        public RecordData GetRecord(string id)
        {
            return _data.GetRecordData(id);
        }

        public List<string> GetAllRecord()
        {
            return _data.GetAllRecordDatas();
        }

        public RoleData GetRoleData(int id)
        {
            return _data.GetUsingRecord().roleDataDic[id];
        }

        public EquipmentData GetEquipmentData(int id)
        {
            return _data.GetUsingRecord().equipDataDic[id];
        }

        public int[] GetAllEquipments()
        {
            var equipments = new int[_data.GetUsingRecord().equipDataDic.Count];
            var index = 0;
            foreach (var v in _data.GetUsingRecord().equipDataDic)
            {
                equipments[index] = v.Value.UID;
                index += 1;
            }
            return equipments;
        }

        public int[] GetAllRoles()
        {
            var roles = new int[_data.GetUsingRecord().roleDataDic.Count];
            var index = 0;
            foreach (var v in _data.GetUsingRecord().roleDataDic)
            {
                roles[index] = v.Value.UID;
                index += 1;
            }
            return roles;
        }

        public bool IsLevelOpen(int levelID)
        {
            var gd = _data.GetUsingRecord();
            if (gd.levelDataDic.ContainsKey(levelID))
                return true;
            var levelConfig = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID);
            if (0 == levelConfig.LastLevel)
                return true;
            if (IsLevelPass(levelConfig.LastLevel))
                return true;
            return false;
        }

        public bool IsLevelPass(int levelID)
        {
            var gd = _data.GetUsingRecord();
            if (!gd.levelDataDic.ContainsKey(levelID))
                return false;
            return gd.levelDataDic[levelID].Stage >= Enum.LevelStage.Passed;
        }


        public LevelData GetLevelData(int levelID)
        {
            var gd = _data.GetUsingRecord();
            if (!gd.levelDataDic.ContainsKey(levelID))
            {
                _data.GetUsingRecord().levelDataDic[levelID] = new LevelData(levelID);
            }
            return gd.levelDataDic[levelID];
        }

        public void SetLevelData(LevelData levelData)
        {
            var gd = _data.GetUsingRecord();
            gd.levelDataDic[levelData.configId] = levelData;
        }

        public LevelRoleData CreateLevelRoleData(Enum.RoleType type, int UID)
        {
            if (type == Enum.RoleType.Hero)
            {
                var roleData = GetRoleData(UID);
                var equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                foreach (var v in roleData.equipmentDic)
                {
                    equipDataDic.Add(v.Key, GetEquipmentData(v.Value));
                }
                return new LevelRoleData(roleData.UID, roleData.configId, roleData.level, Enum.RoleState.Waiting, equipDataDic, roleData.talentDic);
            }
            else if (type == Enum.RoleType.Enemy)
            {

                var enemyConfig = ConfigMgr.Instance.GetConfig<LevelEnemyConfig>("LevelEnemyConfig", UID);
                var equipDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                for (int j = 0; j < enemyConfig.Equips.Length; j++)
                {
                    var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", enemyConfig.Equips[j]);
                    var equipTypeConfig = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (int)equipConfig.Type);
                    equipDic[equipTypeConfig.Place] = new EquipmentData(0, equipConfig.ID);
                }
                return new LevelRoleData(enemyConfig.ID, enemyConfig.RoleID, enemyConfig.Level, Enum.RoleState.Locked, equipDic, null);
            }
            return null;
        }

        public bool IsNewGameData()
        {
            return _data.GetUsingRecord().isNew;
        }

        public void SetGameDataDirty()
        {
            _data.GetUsingRecord().isNew = false;
        }

        public void AddHero(int configID, int level)
        {
            _data.GetUsingRecord().AddHero(configID, level);
        }

        public void AddItem(SourcePair source)
        {
            if (Enum.SourceType.Hero == source.Type)
                _data.GetUsingRecord().AddHero(source.id, source.value);
            else if (Enum.SourceType.Equip == source.Type)
                _data.GetUsingRecord().AddEquip(source.id);
        }

        public void AddItems(SourcePair[] sources)
        {
            foreach (var v in sources)
                AddItem(v);
        }

        /// region 协议部分----------------------------------------------------------

        /// <summary>
        /// 卸掉装备
        /// </summary>
        public UnwearEquipNDPU UnwearEquip(int roleUID, int equipUID)
        {
            try
            {
                var unwearEquips = new List<int>();

                var roleData = GetRoleData(roleUID);
                var equipData = GetEquipmentData(equipUID);
                roleData.equipmentDic.Remove(equipData.GetPlace());
                unwearEquips.Add(equipUID);

                var forcedCom = equipData.GetForcedCombination();
                if (0 != forcedCom)
                {
                    var forcedComPlace = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (int)forcedCom).Place;
                    if (roleData.equipmentDic.ContainsKey(forcedComPlace))
                    {
                        unwearEquips.Add(roleData.equipmentDic[forcedComPlace]);
                        roleData.equipmentDic.Remove(forcedComPlace);
                    }
                }
                return new UnwearEquipNDPU(Enum.ErrorCode.Success, roleUID, unwearEquips);
            }
            catch
            {
                return new UnwearEquipNDPU(Enum.ErrorCode.Error, 0, null);
            }
        }


        /// <summary>
        /// 穿戴装备
        /// </summary>
        public WearEquipNDPU WearEquip(int roleUID, int equipUID, int forcedComEquipUID)
        {
            try
            {
                var wearEquips = new List<int>();
                var unwearEquips = new List<int>();

                var roleData = GetRoleData(roleUID);
                var equipData = GetEquipmentData(equipUID);

                var combinationDic = new Dictionary<Enum.EquipType, bool>();
                var combinations = equipData.GetCombination();
                if (null != combinations)
                {
                    foreach (var v in combinations)
                    {
                        combinationDic[v] = true;
                    };
                }
                foreach (var v in roleData.equipmentDic)
                {
                    var tempEquipData = GetEquipmentData(v.Value);
                    if (!combinationDic.ContainsKey(tempEquipData.GetType()))
                    {
                        unwearEquips.Add(v.Value);
                    }
                }

                foreach (var v in unwearEquips)
                    UnwearEquip(roleUID, v);

                var allRoles = GetAllRoles();
                foreach (var v in allRoles)
                {
                    foreach (var v1 in GetRoleData(v).equipmentDic)
                    {
                        if (v1.Value == equipUID)
                        {
                            UnwearEquip(v, equipUID);
                            break;
                        }
                    }
                }

                wearEquips.Add(equipUID);
                roleData.equipmentDic.Add(equipData.GetPlace(), equipUID);

                if (0 != forcedComEquipUID)
                {
                    foreach (var v in allRoles)
                    {
                        foreach (var v1 in GetRoleData(v).equipmentDic)
                        {
                            if (v1.Value == forcedComEquipUID)
                            {
                                UnwearEquip(v, forcedComEquipUID);
                                break;
                            }
                        }
                    }

                    wearEquips.Add(forcedComEquipUID);
                    var forcedComEquip = GetEquipmentData(forcedComEquipUID);
                    roleData.equipmentDic.Add(forcedComEquip.GetPlace(), forcedComEquipUID);
                }

                return new WearEquipNDPU(Enum.ErrorCode.Success, roleUID, wearEquips, unwearEquips);
            }
            catch (Exception e)
            {
                DebugManager.Instance.Log(e);
                return new WearEquipNDPU(Enum.ErrorCode.Error, 0, null, null);
            }
        }


        public void HeroTalentActiveC2S(int roleUID, int talentID)
        {
            var config = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", talentID);
            var roleData = GetRoleData(roleUID);
            if (null == roleData.talentDic)
                roleData.talentDic = new Dictionary<int, int>();
            roleData.talentDic.Add(config.Place, talentID);

            EventDispatcher.Instance.PostEvent(Enum.EventType.HeroTalentActiveS2C, new object[] { roleUID, talentID });
        }

        /// endregion -----------------------------------------------------------------------------------------------------
    }
}
