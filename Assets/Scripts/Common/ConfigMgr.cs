using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class ConfigMgr : Singeton<ConfigMgr>
    {
        private Dictionary<string, Dictionary<int, Config>> _configDic = new Dictionary<string, Dictionary<int, Config>>();
        private Dictionary<string, TranslationConfig> _TranslationDic = new Dictionary<string, TranslationConfig>();

        public override bool Init()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.SetLanguageS2C, OnLanguageChanged);
            return true;
        }

        private bool InitConfig<T>(string jsonName) where T : Config
        {
            //DebugManager.Instance.Log(jsonName);
            var configs = Tool.Instance.ReadJson<T[]>(Application.streamingAssetsPath + "/Configs/" + jsonName + ".json");
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
            _TranslationDic.Clear();
            var config = GetConfig<LanguageConfig>("LanguageConfig", DatasMgr.Instance.GetLanguage());
            var configs = Tool.Instance.ReadJson<TranslationConfig[]>(Application.streamingAssetsPath + "/Configs/Translation_" + config.SimpleName + ".json");
            foreach (var v in configs)
            {
                _TranslationDic[v.ID] = v;
            }
            return true;
        }


        public string GetTranslation(string configName, int id, string key)
        {
            if (_TranslationDic.Count <= 0)
                InitTranslation();

            key = configName + "_" + id + "_" + key;
            if (!_TranslationDic.ContainsKey(key))
                return "";
            return _TranslationDic[key].Str;
        }

        public string GetTranslation(string key)
        {
            if (_TranslationDic.Count <= 0)
                InitTranslation();

            if (!_TranslationDic.ContainsKey(key))
                return "";
            return _TranslationDic[key].Str;
        }

        private void OnLanguageChanged(params object[] args)
        {
            _TranslationDic.Clear();
            EventDispatcher.Instance.PostEvent(Enum.Event.Language_Changed);
        }

        public override bool Dispose()
        {
            foreach (var pair in _configDic)
            {
                pair.Value.Clear();
            }
            _configDic.Clear();
            EventDispatcher.Instance.RemoveListener(Enum.Event.SetLanguageS2C, OnLanguageChanged);
            return base.Dispose();
        }
    }
}
