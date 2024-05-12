using System.Collections.Generic;
using UnityEngine;
using System;

namespace WarGame
{
    public class DatasMgr : Singeton<DatasMgr>
    {
        private Dictionary<string, GameData> _dataDic;
        private string _curData;
        private string _path = Application.dataPath + "/Datas/GameData.json";

        public override bool Init()
        {
            base.Init();

            _dataDic = Tool.Instance.ReadJson<Dictionary<string, GameData>>(_path);
            if (null == _dataDic)
                _dataDic = new Dictionary<string, GameData>();

            return true;
        }

        public void NewGameData(string name)
        {
            _curData = name;
            var gd = new GameData(name, Time.time);
            _dataDic[_curData] = gd;

            SaveGameData();
        }

        public void ReadGameData(string name)
        {
            _curData = name;
        }

        public void SaveGameData(string name = null)
        {
            GameData gd = null;
            if (null != name)
            {
                gd = _dataDic[_curData].Clone();
                _dataDic[name] = gd;
            }
            else
            {
                gd = _dataDic[_curData];
            }
            gd.time = Time.time;

            Tool.Instance.WriteJson<Dictionary<string, GameData>>(_path, _dataDic);
        }

        public List<SampleGameData> GetGameDatas()
        {
            List<SampleGameData> gameDatas = new List<SampleGameData>();
            foreach (var v in _dataDic)
            {
                gameDatas.Add(new SampleGameData(v.Value.title, v.Value.time));
            }
            return gameDatas;
        }

        public RoleData GetRoleData(int id)
        {
            return _dataDic[_curData].roleDataDic[id];
        }

        public EquipmentData GetEquipmentData(int id)
        {
            return _dataDic[_curData].equipDataDic[id];
        }

        public int[] GetAllEquipments()
        {
            var equipments = new int[_dataDic[_curData].equipDataDic.Count];
            var index = 0;
            foreach (var v in _dataDic[_curData].equipDataDic)
            {
                equipments[index] = v.Value.UID;
                index += 1;
            }
            return equipments;
        }

        public int[] GetAllRoles()
        {
            var roles = new int[_dataDic[_curData].roleDataDic.Count];
            var index = 0;
            foreach (var v in _dataDic[_curData].roleDataDic)
            {
                roles[index] = v.Value.UID;
                index += 1;
            }
            return roles;
        }

        public bool IsLevelOpen(int levelID)
        {
            var gd = _dataDic[_curData];
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
            var gd = _dataDic[_curData];
            if (!gd.levelDataDic.ContainsKey(levelID))
                return false;
            return gd.levelDataDic[levelID].pass;
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

                wearEquips.Add(equipUID);
                roleData.equipmentDic.Add(equipData.GetPlace(), equipUID);

                if (0 != forcedComEquipUID)
                {
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


        public void ActiveTalent(int roleUID, int talentID)
        {
            var config = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", talentID);
            var roleData = GetRoleData(roleUID);
            if (null == roleData.talentDic)
                roleData.talentDic = new Dictionary<int, int>();
            roleData.talentDic.Add(config.Place, talentID);
        }

        public void StartLevel(int levelID)
        {
            var gd = _dataDic[_curData];
            if (!gd.levelDataDic.ContainsKey(levelID))
            {
                gd.levelDataDic[levelID] = new LevelData(levelID);
            }
        }

        public void CompleteLevel(int levelID)
        {
            var gd = _dataDic[_curData];
            gd.levelDataDic[levelID].pass = true;
        }

        /// endregion -----------------------------------------------------------------------------------------------------

        public override bool Dispose()
        {
            SaveGameData();
            _dataDic.Clear();
            base.Dispose();
            return true;
        }
    }
}
