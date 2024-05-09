using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WarGame
{
    public delegate void WGConfigCallback(Config config);

    public class ConfigMgr : Singeton<ConfigMgr>
    {
        private Dictionary<string, Dictionary<int, Config>> _configDic = new Dictionary<string, Dictionary<int, Config>>();

        private bool InitConfig<T>(string jsonName) where T : Config
        {
            var configs = Tool.Instance.ReadJson<T[]>(Application.dataPath + "/Configs/" + jsonName + ".json");
            _configDic[jsonName] = new Dictionary<int, Config>();
            foreach (var v in configs)
            {
                _configDic[jsonName][v.ID] = v;
            }
            return true;
        }

        public T GetConfig<T>(string name, int id) where T : Config
        {
            if (!_configDic.ContainsKey(name))
            {
                if (!InitConfig<T>(name))
                    return default(T);
            }
            
            if (_configDic[name].ContainsKey(id))
                return (T)_configDic[name][id];

            return default(T);
        }

        public void ForeachConfig<T>(string name, WGConfigCallback callback) where T:Config
        {
            if (!_configDic.ContainsKey(name))
            {
                InitConfig<T>(name);
            }
            foreach (var v in _configDic[name])
            {
                callback(v.Value);
            }
        }

        public override bool Dispose()
        {
            base.Dispose();
            foreach (var pair in _configDic)
            {
                pair.Value.Clear();
            }
            _configDic.Clear();
            return true;
        }
    }
}
