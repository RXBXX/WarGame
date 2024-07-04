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
        private List<int> _languageData = new List<int>();
        private Enum.SettingsType _settingType;
        private Dictionary<string, SettingsAudioItem> _audioDic = new Dictionary<string, SettingsAudioItem>();
        private Dictionary<string, SettingsLanguageItem> _languageDic = new Dictionary<string, SettingsLanguageItem>();

        public SettingsPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            _tabList = GetGObjectChild<GList>("tabList");
            _tabList.itemRenderer = OnTabRenderer;
            _tabList.onClickItem.Add(OnClickTab);

            _itemList = GetGObjectChild<GList>("itemList");
            _itemList.SetVirtual();
            _itemList.itemProvider = OnItemProvider;
            _itemList.itemRenderer = OnItemRenderer;

            GetGObjectChild<GButton>("closeBtn").onClick.Add(OnClickClose);
            Init();
        }

        private void Init()
        {
            foreach (var v in System.Enum.GetValues(typeof(Enum.SettingsType)))
                _tabData.Add((Enum.SettingsType)v);

            _tabList.numItems = _tabData.Count;

            _soundData.Clear();
            _soundData.Add(Enum.SoundType.Music);
            _soundData.Add(Enum.SoundType.Audio);
            _itemList.numItems = _soundData.Count;
        }

        private void OnTabRenderer(int index, GObject item)
        {
            if (_tabData[index] == Enum.SettingsType.Sound)
                ((GButton)item).title = "“Ù–ß";
            else
                ((GButton)item).title = "”Ô—‘";
        }

        private void OnClickTab(EventContext context)
        {
            var index = _tabList.GetChildIndex((GObject)context.data);
            if (_settingType == _tabData[index])
                return;
            _settingType = _tabData[index];

            if (_settingType == Enum.SettingsType.Sound)
            {
                _soundData.Clear();
                _soundData.Add(Enum.SoundType.Music);
                _soundData.Add(Enum.SoundType.Audio);
                _itemList.numItems = _soundData.Count;
            }
            else
            {
                _languageData.Clear();
                ConfigMgr.Instance.ForeachConfig<LanguageConfig>("LanguageConfig", (config) =>
                {
                    _languageData.Add(config.ID);
                    _itemList.numItems = _languageData.Count;
                });
            }
        }

        private string OnItemProvider(int index)
        {
            if (_settingType == Enum.SettingsType.Sound)
                return "ui://Settings/SettingsAudioItem";
            else
                return "ui://Settings/SettingsLanguageItem";
        }

        private void OnItemRenderer(int index, GObject item)
        {
            if (_settingType == Enum.SettingsType.Sound)
            {
                if (!_audioDic.ContainsKey(item.id))
                {
                    _audioDic.Add(item.id, UIManager.Instance.CreateUI<SettingsAudioItem>("SettingsAudioItem", item));
                }
                _audioDic[item.id].UpdateItem(_soundData[index]);
            }
            else
            {
                if (!_languageDic.ContainsKey(item.id))
                {
                    _languageDic.Add(item.id, UIManager.Instance.CreateUI<SettingsLanguageItem>("SettingsLanguageItem", item));
                }
                _languageDic[item.id].UpdateItem(_languageData[index]);
            }
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
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
        }
    }
}
