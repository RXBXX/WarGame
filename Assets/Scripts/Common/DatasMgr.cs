using System.Collections.Generic;
using UnityEngine;

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

        public override bool Dispose()
        {
            SaveGameData();
            _dataDic.Clear();
            base.Dispose();
            return true;
        }
    }
}
