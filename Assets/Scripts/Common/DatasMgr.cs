using System.Collections.Generic;
using UnityEngine;
using System;

namespace WarGame
{
    public class DatasMgr : Singeton<DatasMgr>
    {
        private GameData _data = null;
        private string _path = Application.temporaryCachePath + "/GameData.json";
        private float _autoSaveInterval = 300.0f;
        private float _autoTime = 0;

        public override bool Init()
        {
            base.Init();
            _data = Tool.Instance.ReadJson<GameData>(_path);
            if (null == _data)
                _data = new GameData();

            return true;
        }

        public override void Update(float deltaTime)
        {
            _autoTime += deltaTime;
            if (_autoTime > _autoSaveInterval)
            {
                Save();
                _autoTime = 0;
            }
            _data.Update(deltaTime);
        }


        public void Save()
        {
            _data.Save();
            Tool.Instance.WriteJson<GameData>(_path, _data);
            //TipsMgr.Instance.Add(ConfigMgr.Instance.GetTranslation("Data_Saved"));
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
                equipments[index] = v.Key;
                index += 1;
            }
            return equipments;
        }

        public List<int> GetAllRoles()
        {
            var roles = new List<int>();
            foreach (var v in _data.GetUsingRecord().roleDataDic)
            {
                roles.Add(v.Value.UID);
            }
            return roles;
        }

        public int GetItem(int itemId)
        {
            var data = _data.GetUsingRecord();
            if (!data.itemsDic.ContainsKey(itemId))
                return 0;
            return data.itemsDic[itemId];
        }

        public int GetEquipCount(int configID)
        {
            var count = 0;
            foreach (var v in _data.GetUsingRecord().equipDataDic)
            {
                if (v.Value.GetConfigID() == configID)
                    count++;
            }
            return count;
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

        public bool IsLevelEntered(int levelID)
        {
            if (!IsLevelOpen(levelID))
                return false;
            var levelData = GetLevelData(levelID);
            return levelData.Stage >= Enum.LevelStage.Entered;
        }

        public bool IsLevelPass(int levelID)
        {
            var gd = _data.GetUsingRecord();
            if (!gd.levelDataDic.ContainsKey(levelID))
                return false;
            if (gd.levelDataDic[levelID].Stage == Enum.LevelStage.Failed)
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

        public void SetHomeEvent(int eventID)
        {
            _data.GetUsingRecord().homeEvent = eventID;
        }

        public int GetHomeEvent()
        {
            return _data.GetUsingRecord().homeEvent;
        }

        public bool IsNewGameData()
        {
            return _data.GetUsingRecord().isNew;
        }

        public void SetGameDataDirty()
        {
            _data.GetUsingRecord().isNew = false;
        }

        public void AddHero(int id)
        {
            _data.GetUsingRecord().AddHero(id);
        }

        //public void AddEquip(int id)
        //{
        //    _data.GetUsingRecord().AddEquip(id);
        //}

        public bool ContainEquip(int id)
        {
            return _data.GetUsingRecord().ContainEquip(id);
        }

        public void AddItem(int id, int value)
        {
            _data.GetUsingRecord().AddItem(id, value);
        }

        public void AddItems(List<TwoIntPair> sources)
        {
            foreach (var v in sources)
                _data.GetUsingRecord().AddItem(v.id, v.value);
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

        public bool IsHeroTalentActive(int roleUID, int talentID)
        {
            var roleData = GetRoleData(roleUID);
            if (null == roleData.talents)
                return false;

            return roleData.talents.Contains(talentID);
        }

        public bool CanHeroTalentActive(int roleUID, int talentID)
        {
            if (IsHeroTalentActive(roleUID, talentID))
                return false;

            var talentConfig = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", talentID);
            if (0 == talentConfig.LastTalent)
                return true;

            if (IsHeroTalentActive(roleUID, talentConfig.LastTalent))
                return true;

            return false;
        }

        public float GetSoundVolume(Enum.SoundType type)
        {
            return _data.SoundVolumeDic[type];
        }

        public float SetSoundVolume(Enum.SoundType type, float volume)
        {
            _data.SoundVolumeDic[type] = volume;
            return volume;
        }

        public void SetLanguageC2S(int type)
        {
            _data.Language = type;
            EventDispatcher.Instance.PostEvent(Enum.Event.SetLanguageS2C);
        }

        public int GetLanguage()
        {
            return _data.Language;
        }

        public void SetSkipBattle(bool skipBattle)
        {
            _data.SkipBattle = skipBattle;
        }

        public bool GetSkipBattle()
        {
            return _data.SkipBattle;
        }

        public void SetSelectedHeros(List<int> selectedHeros)
        {
            _data.GetUsingRecord().SelectedHeros = selectedHeros;
        }

        public List<int> GetSelectedHeros()
        {
            return _data.GetUsingRecord().SelectedHeros;
        }

        public float GetGameTime()
        {
            return _data.GetUsingRecord().Time;
        }

        /// region 协议部分----------------------------------------------------------

        /// <summary>
        /// 卸掉装备
        /// </summary>
        public void UnwearEquipC2S(int roleUID, int equipUID, bool fromWear = false)
        {
            try
            {
                if (!fromWear)
                    EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_Before);

                var unwearEquips = new List<int>();

                var roleData = GetRoleData(roleUID);
                var equipData = GetEquipmentData(equipUID);
                roleData.equipmentDic.Remove(equipData.GetPlace());
                unwearEquips.Add(equipUID);

                EventDispatcher.Instance.PostEvent(Enum.Event.UnwearEquipS2C, new object[] { new UnwearEquipNDPU(Enum.ErrorCode.Success, roleUID, unwearEquips, fromWear) });

                Save();

                if (!fromWear)
                    EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_After);
            }
            catch (Exception e)
            {
                DebugManager.Instance.LogError(e);
                EventDispatcher.Instance.PostEvent(Enum.Event.UnwearEquipS2C, new object[] { new UnwearEquipNDPU(Enum.ErrorCode.Error, 0, null, fromWear) });
            }
        }


        /// <summary>
        /// 穿戴装备
        /// </summary>
        public void WearEquipC2S(int roleUID, int equipUID)
        {
            try
            {
                EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_Before);

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
                    UnwearEquipC2S(roleUID, v, true);

                //从装备之前的佩戴者身上卸载要穿带的装备
                var allRoles = GetAllRoles();
                foreach (var v in allRoles)
                {
                    foreach (var v1 in GetRoleData(v).equipmentDic)
                    {
                        if (v1.Value == equipUID)
                        {
                            UnwearEquipC2S(v, equipUID, true);
                            break;
                        }
                    }
                }

                wearEquips.Add(equipUID);
                roleData.equipmentDic.Add(equipData.GetPlace(), equipUID);

                EventDispatcher.Instance.PostEvent(Enum.Event.WearEquipS2C, new object[] { new WearEquipNDPU(Enum.ErrorCode.Success, roleUID, wearEquips) });

                Save();

                EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_After);
            }
            catch (Exception e)
            {
                DebugManager.Instance.Log(e);
                EventDispatcher.Instance.PostEvent(Enum.Event.WearEquipS2C, new object[] { new WearEquipNDPU(Enum.ErrorCode.Error, 0, null) });
            }
        }


        public void HeroTalentActiveC2S(int roleUID, int talentID)
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_Before);

            var roleData = GetRoleData(roleUID);

            if (null == roleData.talents)
                roleData.talents = new List<int>();

            if (roleData.talents.Contains(talentID))
                return;

            roleData.talents.Add(talentID);

            var config = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", talentID);
            _data.GetUsingRecord().RemoveItem((int)Enum.ItemType.TalentRes, config.Cost);

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroTalentActiveS2C, new object[] { roleUID, talentID });

            Save();

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_After);
        }

        public void ActiveLevelC2S(int levelID)
        {
            if (_data.GetUsingRecord().levelDataDic.ContainsKey(levelID))
                return;

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_Before);

            var levelData = new LevelData(levelID);
            _data.GetUsingRecord().levelDataDic.Add(levelData.configId, levelData);

            EventDispatcher.Instance.PostEvent(Enum.Event.ActiveLevelS2C, new object[] { levelID });

            Save();

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_After);
        }

        public void HeroLevelUpC2S(int roleUID, int level)
        {
            var roleData = GetRoleData(roleUID);
            if (roleData.GetStarConfig().Cost > GetItem((int)Enum.ItemType.LevelRes))
                return;

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_Before);

            _data.GetUsingRecord().RemoveItem((int)Enum.ItemType.LevelRes, roleData.GetStarConfig().Cost);
            roleData.level = level;
            EventDispatcher.Instance.PostEvent(Enum.Event.HeroLevelUpS2C, new object[] { roleUID, level });

            Save();

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_After);
        }

        public void BuyEquipC2S(int id)
        {
            var cost = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", id).Cost;
            if (GetItem((int)Enum.ItemType.EquipRes) < cost)
            {
                EventDispatcher.Instance.PostEvent(Enum.Event.BuyEquipS2C, new object[] { Enum.ErrorCode.SrcNotEnough });
            }
            else
            {
                _data.GetUsingRecord().RemoveItem((int)Enum.ItemType.EquipRes, cost);
                var equipID = _data.GetUsingRecord().AddEquip(id);
                EventDispatcher.Instance.PostEvent(Enum.Event.BuyEquipS2C, new object[] { Enum.ErrorCode.Success, equipID });
                Save();
            }
        }

        public void DeleteRecordC2S(string id)
        {
            _data.DeleteRecord(id);
            Save();
            EventDispatcher.Instance.PostEvent(Enum.Event.DeleteRecordS2C, new object[] { id });
        }

        public void ResetHeroC2S(int roleUID)
        {
            var roleData = GetRoleData(roleUID);

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_Before);

            //重置等级
            var level = roleData.level;
            if (level >= 1)
            {
                var levelRes = 0;
                for (int i = level - 1; i >= 1; i--)
                {
                    var levelConfig = ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", roleData.configId * 1000 + i);
                    levelRes += levelConfig.Cost;
                }
                roleData.level = 1;
                _data.GetUsingRecord().AddItem((int)Enum.ItemType.LevelRes, levelRes);
            }

            //重置天赋
            if (roleData.talents.Count > 0)
            {
                var talentRes = 0;
                foreach (var v in roleData.talents)
                {
                    talentRes += ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", v).Cost;
                }
                roleData.talents.Clear();
                _data.GetUsingRecord().AddItem((int)Enum.ItemType.TalentRes, talentRes);
            }

            EventDispatcher.Instance.PostEvent(Enum.Event.ResetHeroS2C, new object[] { Enum.ErrorCode.Success, roleUID });
            Save();

            EventDispatcher.Instance.PostEvent(Enum.Event.HeroChange_After);
        }

        /// endregion -----------------------------------------------------------------------------------------------------
    }
}
