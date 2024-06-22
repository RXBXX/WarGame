using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace WarGame
{
    public class ConfigMgr : Singeton<ConfigMgr>
    {
        private string _language = "zh";
        private Dictionary<string, Dictionary<int, Config>> _configDic = new Dictionary<string, Dictionary<int, Config>>();
        private Dictionary<string, TranslationConfig> _TranslationDic;

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

        private bool InitTranslation()
        {
            _TranslationDic = new Dictionary<string, TranslationConfig>();
            var configs = Tool.Instance.ReadJson<TranslationConfig[]>("Assets/StreamingAssets/Configs/Translation_" + _language + ".json");
            foreach (var v in configs)
            {
                _TranslationDic[v.ID] = v;
            }
            return true;
        }

        public string GetTranslation(string configName, int id, string key)
        {
            if (null == _TranslationDic)
                InitTranslation();

            key = configName + "_" + id + "_" + key;
            if (!_TranslationDic.ContainsKey(key))
                return "";
            return _TranslationDic[key].Str;
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
