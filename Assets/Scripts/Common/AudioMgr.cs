using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class AudioMgr : Singeton<AudioMgr>
    {
        private class AudioPair
        {
            public int id;
            public long time;
            public long length;
            public AudioSource audioSource;
            public int assetID;
            public AudioClip audioClip;
            public bool isLoop;

            public AudioPair(int id, long time, AudioSource audioSource)
            {
                this.id = id;
                this.time = time;
                this.audioSource = audioSource;
            }

            public void SetLoop(bool isLoop)
            {
                this.isLoop = isLoop;
                audioSource.loop = isLoop;
            }

            public void LoadClip(string clipPath)
            {
                assetID = AssetsMgr.Instance.LoadAssetAsync<AudioClip>(clipPath, (clip) =>
                {
                    length = (long)(clip.length * 1000) + 1000;
                    audioClip = clip;
                    audioSource.volume = DatasMgr.Instance.GetSoundVolume(Enum.SoundType.Audio);
                    audioSource.clip = clip;
                    audioSource.Play();
                });
            }

            public void SetActive(bool active)
            {
                audioSource.enabled = active;
                //DebugManager.Instance.Log("AudioSource Enabled:" + audioSource.enabled);
            }

            public void SetVolume(float volume)
            {
                audioSource.volume = volume;
            }

            public bool IsOver()
            {
                if (null == audioClip)
                    return false;
                return time + length < TimeMgr.Instance.GetUnixTimestamp();
            }

            public void Dispose()
            {
                SetActive(false);
                audioClip = null;
                audioSource.clip = null;
                length = 0;

                AssetsMgr.Instance.ReleaseAsset(assetID);
                assetID = 0;

                //if (null != audioClip)
                //{
                //    AssetsMgr.Instance.Destroy(audioClip);
                //    audioClip = null;
                //}
            }
        }

        private GameObject _GO;
        private AudioSource _backgoundAS;
        private List<AudioPair> _soundASs = new List<AudioPair>();
        private Stack<AudioPair> _audioStack = new Stack<AudioPair>();
        private int _count;

        public override bool Init()
        {
            _GO = GameObject.Find("Entrance");
            return base.Init();
        }

        public void PlayMusic(string music)
        {
            if (null == _backgoundAS)
            {
                _backgoundAS = _GO.AddComponent<AudioSource>();
                _backgoundAS.volume = DatasMgr.Instance.GetSoundVolume(Enum.SoundType.Music);
                _backgoundAS.loop = true;
            }

            AssetsMgr.Instance.LoadAssetAsync<AudioClip>(music, (clip) =>
            {
                _backgoundAS.clip = clip;
                _backgoundAS.Play();
            });
        }

        public int PlaySound(string sound, bool isLoop = false)
        {
            //DebugManager.Instance.Log(sound);
            var audioSource = CreateAudioSource();
            audioSource.SetVolume(DatasMgr.Instance.GetSoundVolume(Enum.SoundType.Audio));
            audioSource.LoadClip(sound);
            audioSource.SetLoop(isLoop);
            _soundASs.Add(audioSource);

            return audioSource.id;
        }

        public void StopSound(int id)
        {
            for (int i = _soundASs.Count - 1; i >= 0; i--)
            {
                if (_soundASs[i].id == id)
                {
                    _soundASs[i].Dispose();
                    _audioStack.Push(_soundASs[i]);
                    _soundASs.RemoveAt(i);
                    break;
                }
            }
        }

        public void ChangeVolume(Enum.SoundType type, float volume)
        {
            if (volume < 0 || volume > 1)
                return;

            volume = DatasMgr.Instance.SetSoundVolume(type, volume);

            if (type == Enum.SoundType.Music)
            {
                _backgoundAS.volume = volume;
            }
            else
            {
                foreach (var v in _soundASs)
                    v.SetVolume(volume);
            }
        }

        private AudioPair CreateAudioSource()
        {
            AudioPair audioSource = null;
            if (_audioStack.Count > 0)
            {
                audioSource = _audioStack.Pop();
                audioSource.time = TimeMgr.Instance.GetUnixTimestamp();
                audioSource.SetActive(true);
            }
            else
            {
                _count++;
                audioSource = new AudioPair(_count, TimeMgr.Instance.GetUnixTimestamp(), _GO.AddComponent<AudioSource>());
            }

            return audioSource;
        }

        public override void Update(float deltaTime)
        {
            for (int i = _soundASs.Count - 1; i >= 0; i--)
            {
                if (_soundASs[i].isLoop)
                    continue;

                if (_soundASs[i].IsOver())
                {
                    _soundASs[i].Dispose();
                    _audioStack.Push(_soundASs[i]);
                    _soundASs.RemoveAt(i);
                }
            }
        }

        public void ChangeAudioListener(Enum.AudioListenerType type)
        {
            if (type == Enum.AudioListenerType.TwoD)
            {
                GameObject.Find("Entrance").GetComponent<AudioListener>().enabled = true;
            }
            else
            {
            
            }
        }

        public override bool Dispose()
        {
            foreach (var v in _soundASs)
                v.Dispose();
            _soundASs.Clear();

            foreach (var v in _audioStack)
                v.Dispose();
            _audioStack.Clear();

            return base.Dispose();
        }
    }
}
