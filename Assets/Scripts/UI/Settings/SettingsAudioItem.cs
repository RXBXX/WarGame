using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class SettingsAudioItem : UIBase
    {
        private Enum.SoundType _type;
        private GTextField _titie;
        private GSlider _slider;

        public SettingsAudioItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _titie = GetGObjectChild<GTextField>("title");
            _slider = GetGObjectChild<GSlider>("slider");

            _slider.onChanged.Add(OnChange);
        }

        public void UpdateItem(Enum.SoundType type)
        {
            _type = type;
            if (type == Enum.SoundType.Music)
                _titie.text = ConfigMgr.Instance.GetTranslation("SettingsPanel_Audio_Music");
            else
                _titie.text = ConfigMgr.Instance.GetTranslation("SettingsPanel_Audio_Sound");

            _slider.value = DatasMgr.Instance.GetSoundVolume(type) * 100.0F;
        }

        private void OnChange(EventContext context)
        {
            AudioMgr.Instance.ChangeVolume(_type, (float)_slider.value / 100.0f);
        }
    }
}
