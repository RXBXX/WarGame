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
            return _dataDic[_curData].roleData[id];
        }

        public EquipmentData GetEquipmentData(int id)
        {
            return _dataDic[_curData].equipmentData[id];
        }

        public int[] GetAllEquipments()
        {
            var equipments = new int[_dataDic[_curData].equipmentData.Count];
            var index = 0;
            foreach (var v in _dataDic[_curData].equipmentData)
            {
                equipments[index] = v.Value.UID;
                index += 1;
            }
            return equipments;
        }

        public int[] GetAllRoles()
        {
            var roles = new int[_dataDic[_curData].roleData.Count];
            var index = 0;
            foreach (var v in _dataDic[_curData].roleData)
            {
                roles[index] = v.Value.UID;
                index += 1;
            }
            return roles;
        }

        public override bool Dispose()
        {
            base.Dispose();

            _dataDic.Clear();

            return true;
        }
    }
}
