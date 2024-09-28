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
            public GameObject GO;

            public AudioPair(int id, long time, AudioSource audioSource, GameObject go)
            {
                this.id = id;
                this.time = time;
                this.audioSource = audioSource;
                this.GO = go;
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
//                MissingReferenceException: The object of type 'AudioSource' has been destroyed but you are still trying to access it.
//Your script should either check if it is null or you should not destroy the object.
//WarGame.AudioMgr + AudioPair.< LoadClip > b__10_0(UnityEngine.AudioClip clip)(at Assets / Scripts / Common / AudioMgr.cs:40)
//WarGame.AssetsMgr +< Load > d__7`1[T].MoveNext()(at Assets / Scripts / Common / AssetsMgr.cs:148)
//UnityEngine.SetupCoroutine.InvokeMoveNext(System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress)(at < 26a395a3682048b6b60924a5f0435897 >:0)
                    audioSource.clip = clip;
                    audioSource.Play();
                });
            }

            public void SetActive(bool active)
            {
                audioSource.enabled = active;
            }

            public void SetVolume(float volume)
            {
                audioSource.volume = volume;
            }

            public void SetBlend(float blend)
            {
                audioSource.spatialBlend = blend;
            }

            public void SetMinDictance(float distance)
            {
                audioSource.minDistance = distance;
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
            }
        }

        private GameObject _GO;
        private AudioSource _backgoundAS;
        private List<AudioPair> _soundASs = new List<AudioPair>();
        private List<AudioPair> _audioPool = new List<AudioPair>();
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


        public int PlaySound(string sound, bool isLoop = false, GameObject go = null, float blend = 0, float minDistance = 0)
        {
            if (null == go)
                go = _GO;

            var audioSource = CreateAudioSource(go);
            audioSource.SetBlend(blend);
            audioSource.SetMinDictance(minDistance);
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
                    _audioPool.Add(_soundASs[i]);
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

        private AudioPair CreateAudioSource(GameObject go)
        {
            AudioPair audioSource = null;
            if (_audioPool.Count > 0)
            {
                for (int i = _audioPool.Count - 1; i >= 0; i--)
                {
                    if (_audioPool[i].GO == go)
                    {
                        audioSource = _audioPool[i];
                        _audioPool.RemoveAt(i);
                        break;
                    }
                }
            }
            if (null == audioSource)
            {
                _count++;
                audioSource = new AudioPair(_count, TimeMgr.Instance.GetUnixTimestamp(), go.AddComponent<AudioSource>(), go);
            }
            audioSource.time = TimeMgr.Instance.GetUnixTimestamp();
            audioSource.SetActive(true);

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
                    _audioPool.Add(_soundASs[i]);
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

        public void ClearSound(GameObject go)
        {
            for (int i = _soundASs.Count - 1; i >= 0; i--)
            {
                if (_soundASs[i].GO = go)
                    _soundASs.RemoveAt(i);
            }

            for (int i = _audioPool.Count - 1; i >= 0; i--)
            {
                if (_audioPool[i].GO = go)
                    _audioPool.RemoveAt(i);
            }
        }

        public override bool Dispose()
        {
            foreach (var v in _soundASs)
                v.Dispose();
            _soundASs.Clear();

            foreach (var v in _audioPool)
                v.Dispose();
            _audioPool.Clear();

            return base.Dispose();
        }
    }
}
