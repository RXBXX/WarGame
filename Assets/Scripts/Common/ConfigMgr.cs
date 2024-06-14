using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace WarGame
{
    public class ConfigMgr : Singeton<ConfigMgr>
    {
        private Dictionary<string, Dictionary<int, Config>> _configDic = new Dictionary<string, Dictionary<int, Config>>();

        private bool InitConfig<T>(string jsonName) where T : Config
        {
            var configs = Tool.Instance.ReadJson<T[]>("Assets/StreamingAssets/Configs/" + jsonName + ".json");
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

        public void ForeachConfig<T>(string name, WGConfigCallback<T> callback) where T : Config
        {
            if (!_configDic.ContainsKey(name))
            {
                InitConfig<T>(name);
            }
            foreach (var v in _configDic[name])
            {
                callback((T)v.Value);
            }
        }

        public override bool Dispose()
        {
            foreach (var pair in _configDic)
            {
                pair.Value.Clear();
            }
            _configDic.Clear();
            return base.Dispose();
        }
    }
}
