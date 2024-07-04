using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class AudioMgr : Singeton<AudioMgr>
    {
        private AudioSource _backgoundAudioSource;
        private AudioSource _soundAudioSource;

        public override bool Init()
        {
            var entrance = GameObject.Find("Entrance");
            var audioSources = entrance.GetComponents<AudioSource>();
            _backgoundAudioSource = audioSources[0];
            _backgoundAudioSource.volume = DatasMgr.Instance.GetSoundVolume(Enum.SoundType.Music);
            _soundAudioSource = audioSources[1];
            _soundAudioSource.volume = DatasMgr.Instance.GetSoundVolume(Enum.SoundType.Audio);

            return base.Init();
        }

        public void PlayMusic(string music)
        {
            AssetsMgr.Instance.LoadAssetAsync<AudioClip>(music, (clip) =>
            {
                _backgoundAudioSource.clip = clip;
                _backgoundAudioSource.Play();
            });
        }

        public void PlaySound(string sound)
        {
            AssetsMgr.Instance.LoadAssetAsync<AudioClip>(sound, (clip) =>
            {
                _soundAudioSource.clip = clip;
            });
        }

        public void ChangeVolume(Enum.SoundType type, float volume)
        {
            if (volume < 0 || volume > 1)
                return;

            volume = DatasMgr.Instance.SetSoundVolume(type, volume);

            if (type == Enum.SoundType.Music)
            {
                _backgoundAudioSource.volume = volume;
            }
            else
            {
                _soundAudioSource.volume = volume;
            }
        }

        public override bool Dispose()
        {
            return base.Dispose();
        }
    }
}
