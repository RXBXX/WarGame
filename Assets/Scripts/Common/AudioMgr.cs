using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class AudioMgr : Singeton<AudioMgr>
    {
        private AudioSource _backgoundAudioSource;
        private AudioSource _soundAudioSource;
        private float _backgroundVolume = 0.05F;
        private float _soundVolume = 0.1F;

        public override bool Init()
        {
            var entrance = GameObject.Find("Entrance");
            var audioSources = entrance.GetComponents<AudioSource>();
            _backgoundAudioSource = audioSources[0];
            _backgoundAudioSource.volume = _backgroundVolume;
            _soundAudioSource = audioSources[1];
            _soundAudioSource.volume = _soundVolume;

            return base.Init();
        }

        public void PlayMusic(string music)
        {
            AssetsMgr.Instance.LoadAssetAsync<AudioClip>(music, (clip)=> {
                _backgoundAudioSource.clip = clip;
                _backgoundAudioSource.Play();
            });
        }

        public void PlaySound(string sound)
        {
            AssetsMgr.Instance.LoadAssetAsync<AudioClip>(sound, (clip) => {
                _soundAudioSource.clip = clip;
            });
        }

        public override bool Dispose()
        {
            return base.Dispose();
        }
    }
}
