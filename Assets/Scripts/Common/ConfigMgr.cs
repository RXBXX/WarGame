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
        private Dictionary<string, TranslationConfig> _TranslationDic = new Dictionary<string, TranslationConfig>();

        public override bool Init()
        {
            var language = Application.systemLanguage;
            switch (language)
            {
                case SystemLanguage.ChineseSimplified:
                    DatasMgr.Instance.SetLanguageC2S(2);
                    break;
                case SystemLanguage.Russian:
                    DatasMgr.Instance.SetLanguageC2S(3);
                    break;
                case SystemLanguage.Spanish:
                    DatasMgr.Instance.SetLanguageC2S(4);
                    break;
                case SystemLanguage.Portuguese:
                    DatasMgr.Instance.SetLanguageC2S(5);
                    break;
                case SystemLanguage.German:
                    DatasMgr.Instance.SetLanguageC2S(6);
                    break;
                case SystemLanguage.Japanese:
                    DatasMgr.Instance.SetLanguageC2S(7);
                    break;
                case SystemLanguage.French:
                    DatasMgr.Instance.SetLanguageC2S(8);
                    break;
                case SystemLanguage.Polish:
                    DatasMgr.Instance.SetLanguageC2S(9);
                    break;
                case SystemLanguage.Korean:
                    DatasMgr.Instance.SetLanguageC2S(10);
                    break;
                case SystemLanguage.ChineseTraditional:
                    DatasMgr.Instance.SetLanguageC2S(11);
                    break;
                case SystemLanguage.Turkish:
                    DatasMgr.Instance.SetLanguageC2S(12);
                    break;
                default:
                    DatasMgr.Instance.SetLanguageC2S(1);
                    break;
            }

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
