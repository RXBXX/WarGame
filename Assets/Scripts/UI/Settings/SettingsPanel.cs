using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class SettingsPanel : UIBase
    {
        private GList _tabList;
        private GList _itemList;
        private List<Enum.SettingsType> _tabData = new List<Enum.SettingsType>();
        private List<Enum.SoundType> _soundData = new List<Enum.SoundType>();
        private List<Enum.LanguageType> _languageData = new List<Enum.LanguageType>();
        private List<Enum.FightType> _fightData = new List<Enum.FightType>();
        private List<int> _tipsData = new List<int>();
        private Enum.SettingsType _settingType;
        private Dictionary<string, SettingsAudioItem> _audioDic = new Dictionary<string, SettingsAudioItem>();
        private Dictionary<string, SettingsLanguageItem> _languageDic = new Dictionary<string, SettingsLanguageItem>();
        private Dictionary<string, SettingsFightItem> _fightDic = new Dictionary<string, SettingsFightItem>();

        public SettingsPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;
            GetGObjectChild<GLoader>("BG").url = "UI/Background/CommonBG";

            _tabList = GetGObjectChild<GList>("tabList");
            _tabList.itemRenderer = OnTabRenderer;
            _tabList.onClickItem.Add(OnClickTab);

            _itemList = GetGObjectChild<GList>("itemList");
            _itemList.SetVirtual();
            _itemList.itemProvider = OnItemProvider;
            _itemList.itemRenderer = OnItemRenderer;

            GetGObjectChild<GButton>("closeBtn").onClick.Add(OnClickClose);
            Init();

            EventDispatcher.Instance.AddListener(Enum.Event.Language_Changed, OnLanguageChanged);
        }

        private void Init()
        {
            _tabData.Clear();

            foreach (var v in System.Enum.GetValues(typeof(Enum.SettingsType)))
                _tabData.Add((Enum.SettingsType)v);

            DebugManager.Instance.Log(_tabData.Count);
            _tabList.numItems = _tabData.Count;

            SelectTab(_settingType);
        }

        private void OnTabRenderer(int index, GObject item)
        {
            switch (_tabData[index])
            {
                case Enum.SettingsType.Sound:
                    ((GButton)item).title = ConfigMgr.Instance.GetTranslation("SettingsPanel_TabSound");
                    break;
                case Enum.SettingsType.Language:
                    ((GButton)item).title = ConfigMgr.Instance.GetTranslation("SettingsPanel_TabLanguage");
                    break;
                case Enum.SettingsType.Fight:
                    ((GButton)item).title = ConfigMgr.Instance.GetTranslation("SettingsPanel_TabBattle");
                    break;
                case Enum.SettingsType.PlayGame:
                    ((GButton)item).title = ConfigMgr.Instance.GetTranslation("SettingsPanel_TabPlayGame");
                    break;
            }
            ((GButton)item).selected = _tabData[index] == _settingType;
        }

        private void OnClickTab(EventContext context)
        {
            var index = _tabList.GetChildIndex((GObject)context.data);
            if (_tabData[index] == _settingType)
                return;
            SelectTab(_tabData[index]);
        }

        private string OnItemProvider(int index)
        {
            switch (_settingType)
            {
                case Enum.SettingsType.Sound:
                    return "ui://Settings/SettingsAudioItem";
                case Enum.SettingsType.Language:
                    return "ui://Settings/SettingsLanguageItem";
                case Enum.SettingsType.Fight:
                    return "ui://Settings/SettingsFightItem";
                case Enum.SettingsType.PlayGame:
                    return "ui://Settings/SettingsTipsItem";
                default:
                    return "";
            }
        }

        private void OnItemRenderer(int index, GObject item)
        {
            switch (_settingType)
            {
                case Enum.SettingsType.Sound:
                    if (!_audioDic.ContainsKey(item.id))
                    {
                        _audioDic.Add(item.id, UIManager.Instance.CreateUI<SettingsAudioItem>("SettingsAudioItem", item));
                    }
                    _audioDic[item.id].UpdateItem(_soundData[index]);
                    break;
                case Enum.SettingsType.Language:
                    if (!_languageDic.ContainsKey(item.id))
                    {
                        _languageDic.Add(item.id, UIManager.Instance.CreateUI<SettingsLanguageItem>("SettingsLanguageItem", item));
                    }
                    _languageDic[item.id].UpdateItem(_languageData[index]);
                    break;
                case Enum.SettingsType.Fight:
                    if (!_fightDic.ContainsKey(item.id))
                    {
                        _fightDic.Add(item.id, UIManager.Instance.CreateUI<SettingsFightItem>("SettingsFightItem", item));
                    }
                    _fightDic[item.id].UpdateItem(_fightData[index]);
                    break;
                case Enum.SettingsType.PlayGame:
                    ((GButton)item).title = ConfigMgr.Instance.GetConfig<PlayGameConfig>("PlayGameConfig", _tipsData[index]).GetTranslation("Desc");
                    break;
            }
        }

        private void SelectTab(Enum.SettingsType tab)
        {
            _settingType = tab;

            switch (_settingType)
            {
                case Enum.SettingsType.Sound:
                    _soundData.Clear();
                    _soundData.Add(Enum.SoundType.Music);
                    _soundData.Add(Enum.SoundType.Audio);
                    _itemList.numItems = _soundData.Count;
                    break;
                case Enum.SettingsType.Language:
                    _languageData.Clear();
                    _languageData.Add(Enum.LanguageType.Text);
                    _itemList.numItems = _languageData.Count;
                    break;
                case Enum.SettingsType.Fight:
                    _fightData.Clear();
                    _fightData.Add(Enum.FightType.SkipBattleArena);
                    _itemList.numItems = _fightData.Count;
                    break;
                case Enum.SettingsType.PlayGame:
                    _tipsData.Clear();
                    ConfigMgr.Instance.ForeachConfig<PlayGameConfig>("PlayGameConfig", (config) =>
                    {
                        _tipsData.Add(config.ID);
                    });
                    _itemList.numItems = _tipsData.Count;
                    break;
            }
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
        }

        private void OnLanguageChanged(params object[] args)
        {
            if (Enum.SettingsType.Language != _settingType)
                return;

            Init();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            foreach (var v in _audioDic)
            {
                v.Value.Dispose();
            }
            _audioDic.Clear();

            foreach (var v in _languageDic)
            {
                v.Value.Dispose();
            }
            _languageDic.Clear();

            foreach (var v in _fightDic)
            {
                v.Value.Dispose();
            }
            _fightDic.Clear();

            EventDispatcher.Instance.RemoveListener(Enum.Event.Language_Changed, OnLanguageChanged);
        }
    }
}
