using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class SettingsLanguageItem : UIBase
    {
        private int _type;
        private GTextField _title;
        private GTextField _value;
        private int _index;
        private List<int> _languages = new List<int>();

        public SettingsLanguageItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _title = GetGObjectChild<GTextField>("title");
            _value = GetGObjectChild<GTextField>("value");

            GetGObjectChild<GButton>("leftBtn").onClick.Add(OnClickLeft);
            GetGObjectChild<GButton>("rightBtn").onClick.Add(OnClickRight);

            ConfigMgr.Instance.ForeachConfig<LanguageConfig>("LanguageConfig", (config)=> {
                _languages.Add(config.ID);
            });
        }

        public void UpdateItem(Enum.LanguageType languageType)
        {
            _title.text = ConfigMgr.Instance.GetTranslation("SettingsPanel_TextLanguage");

            var language = DatasMgr.Instance.GetLanguage();
            for (int i = 0; i < _languages.Count; i++)
            {
                if (_languages[i] == language)
                {
                    _index = i;
                    break;
                }    
            }

            _value.text = ConfigMgr.Instance.GetConfig<LanguageConfig>("LanguageConfig", language).GetTranslation("Name");
        }

        private void OnClickLeft(EventContext context)
        {
            _index -= 1;
            if (_index < 0)
                _index = _languages.Count - 1;
            DebugManager.Instance.Log("Language:" + _languages[_index]);
            DatasMgr.Instance.SetLanguageC2S(_languages[_index]);
        }

        private void OnClickRight(EventContext context)
        {
            _index += 1;
            if (_index >= _languages.Count - 1)
                _index = 0;
            DebugManager.Instance.Log("Language:" + _languages[_index]);
            DatasMgr.Instance.SetLanguageC2S(_languages[_index]);
        }
    }
}
