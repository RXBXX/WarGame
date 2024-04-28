using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WarGame
{
    public class Tool : Singeton<Tool>
    {
        [System.Serializable]
        private class Pack<T>
        {
            public T data;

            public Pack(T data)
            {
                this.data = data;
            }
        }

        ///��ʹ��Unity�ٷ��ṩ��JsonUtility�����JSONת��ʱ������һ��ת������ͻ��������,��Ҫ���������׸���
        ///���ⷢ��UnityEngine.Json��DictionaryҲ��֧��
        public string ToJson<T>(T obj)
        {
            if (typeof(T).GetInterface("IList") != null)
            {
                Pack<T> pack = new Pack<T>(obj);
                pack.data = obj;
                string json = JsonUtility.ToJson(pack);
                return json.Substring(8, json.Length - 9);
            }

            return JsonUtility.ToJson(obj);
        }

        public T FromJson<T>(string json)
        {
            Debug.Log(json);

            if (json == "null" && typeof(T).IsClass) return default(T);
            if (typeof(T).GetInterface("IList") != null)
            {
                json = "{\"data\":{data}}".Replace("{data}", json);
                Pack<T> Pack = JsonUtility.FromJson<Pack<T>>(json);
                return Pack.data;
            }
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// ��ȡjson�ļ�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T ReadJson<T>(string path)
        {
            var jsonStr = "";
            try { jsonStr = File.ReadAllText(path); }
            catch
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(jsonStr);
            //return FromJson<T>(jsonStr);
        }

        /// <summary>
        /// д��json�ļ�
        /// </summary>
        public void WriteJson<T>(string path, T t)
        {
            var jsonStr = JsonConvert.SerializeObject(t);
            try { File.WriteAllText(path, jsonStr); }
            catch (IOException exception)
            {
                Debug.Log(exception);
            };
        }

        public Dictionary<string, float> GetEventTimeForAnimClip(Animator animator, string clipName)
        {
            var timeDic = new Dictionary<string, float>();
            var clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == clipName)
                {
                    float startTime = 0, endTime = 0;
                    for (int j = 0; j < clips[i].events.Length; j++)
                    {
                        timeDic.Add(clips[i].events[j].stringParameter, clips[i].events[j].time);
                    }
                    break;
                }
            }
            return timeDic;
        }
    }
}
