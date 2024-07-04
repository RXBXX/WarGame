using FairyGUI;

namespace WarGame.UI
{
    public class SettingsLanguageItem : UIBase
    {
        private int _type;
        private GTextField _title;

        public SettingsLanguageItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _title = GetGObjectChild<GTextField>("title");

            _gCom.onClick.Add(OnClick);
        }

        public void UpdateItem(int language)
        {
            _type = language;
            _title.text = ConfigMgr.Instance.GetConfig<LanguageConfig>("LanguageConfig", _type).Name;
        }

        private void OnClick()
        {
            DatasMgr.Instance.SetLanguage(_type);
        }
    }
}
