using System.Collections.Generic;
using UnityEngine;
using System;

namespace WarGame
{
    public class DatasMgr : Singeton<DatasMgr>
    {
        private GameData _data = null;
        private string _path = Application.streamingAssetsPath + "/Datas/GameData.json";

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
            //var levelConfig = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID);
            //if (0 == levelConfig.LastLevel)
            //    return true;
            //if (IsLevelPass(levelConfig.LastLevel))
            //    return true;
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

        public LevelRoleData CreateLevelRoleData(Enum.RoleType type, int UID, string bornHexagonID)
        {
            if (type == Enum.RoleType.Hero)
            {
                var roleData = GetRoleData(UID);
                var equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                foreach (var v in roleData.equipmentDic)
                {
                    equipDataDic.Add(v.Key, GetEquipmentData(v.Value));
                }
                return new LevelRoleData(roleData.UID, roleData.configId, roleData.level, bornHexagonID, Enum.RoleState.Waiting, equipDataDic, roleData.talentDic);
            }
            else if (type == Enum.RoleType.Enemy)
            {

                var enemyConfig = ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", UID);
                var equipDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                if (null != enemyConfig.Equips)
                {
                    for (int j = 0; j < enemyConfig.Equips.Length; j++)
                    {
                        var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", enemyConfig.Equips[j]);
                        var equipTypeConfig = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (int)equipConfig.Type);
                        equipDic[equipTypeConfig.Place] = new EquipmentData(0, equipConfig.ID);
                    }
                }
                return new LevelRoleData(enemyConfig.ID, enemyConfig.RoleID, enemyConfig.Level, bornHexagonID, Enum.RoleState.Locked, equipDic, null);
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

        public void AddHero(int heroID)
        {
            _data.GetUsingRecord().AddHero(heroID);
        }

        public void AddItem(SourcePair source)
        {
            if (Enum.SourceType.Hero == source.Type)
                _data.GetUsingRecord().AddHero(source.id);
            else if (Enum.SourceType.Equip == source.Type)
                _data.GetUsingRecord().AddEquip(source.id);
        }

        public void AddItems(SourcePair[] sources)
        {
            foreach (var v in sources)
                AddItem(v);
        }

        public List<int> GetOpenedLevels()
        {
            List<int> levels = new List<int>();
            foreach (var v in _data.GetUsingRecord().levelDataDic)
            {
                levels.Add(v.Value.configId);
            }
            return levels;
        }

        /// region 协议部分----------------------------------------------------------

        /// <summary>
        /// 卸掉装备
        /// </summary>
        public void UnwearEquipC2S(int roleUID, int equipUID)
        {
            try
            {
                var unwearEquips = new List<int>();

                var roleData = GetRoleData(roleUID);
                var equipData = GetEquipmentData(equipUID);
                roleData.equipmentDic.Remove(equipData.GetPlace());
                unwearEquips.Add(equipUID);

                EventDispatcher.Instance.PostEvent(Enum.Event.UnwearEquipS2C, new object[] { new UnwearEquipNDPU(Enum.ErrorCode.Success, roleUID, unwearEquips) });
            }
            catch
            {
                EventDispatcher.Instance.PostEvent(Enum.Event.UnwearEquipS2C, new object[] { new UnwearEquipNDPU(Enum.ErrorCode.Error, 0, null) });
            }
        }


        /// <summary>
        /// 穿戴装备
        /// </summary>
        public void WearEquipC2S(int roleUID, int equipUID)
        {
            try
            {
                var wearEquips = new List<int>();
                var unwearEquips = new List<int>();

                var roleData = GetRoleData(roleUID);
                var equipData = GetEquipmentData(equipUID);

                //卸载英雄身上与要穿带的装备不兼容的其他装备
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
                    UnwearEquipC2S(roleUID, v);

                //从装备之前的佩戴者身上卸载要穿带的装备
                var allRoles = GetAllRoles();
                foreach (var v in allRoles)
                {
                    foreach (var v1 in GetRoleData(v).equipmentDic)
                    {
                        if (v1.Value == equipUID)
                        {
                            UnwearEquipC2S(v, equipUID);
                            break;
                        }
                    }
                }

                wearEquips.Add(equipUID);
                roleData.equipmentDic.Add(equipData.GetPlace(), equipUID);

                EventDispatcher.Instance.PostEvent(Enum.Event.WearEquipS2C, new object[] { new WearEquipNDPU(Enum.ErrorCode.Success, roleUID, wearEquips)});
            }
            catch (Exception e)
            {
                DebugManager.Instance.Log(e);
                EventDispatcher.Instance.PostEvent(Enum.Event.WearEquipS2C, new object[] { new WearEquipNDPU(Enum.ErrorCode.Error, 0, null) });
            }
        }


        public void HeroTalentActiveC2S(int roleUID, int talentID)
        {
            var config = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", talentID);
            var roleData = GetRoleData(roleUID);
            if (null == roleData.talentDic)
                roleData.talentDic = new Dictionary<int, int>();
            roleData.talentDic.Add(config.Place, talentID);

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroTalentActiveS2C, new object[] { roleUID, talentID });
        }

        public void ActiveLevelC2S(int levelID)
        {
            var levelData = new LevelData(levelID);
            _data.GetUsingRecord().levelDataDic.Add(levelData.configId, levelData);
        }

        public void HeroLevelUpC2S(int roleUID, int level)
        {
            var roleData = GetRoleData(roleUID);
            roleData.level = level;

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroLevelUpS2C, new object[] { roleUID, level });
        }

        /// endregion -----------------------------------------------------------------------------------------------------
    }
}
