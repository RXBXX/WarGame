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
            var jsonStr = JsonConvert.SerializeObject(t, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
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
                    for (int j = 0; j < clips[i].events.Length; j++)
                    {
                        timeDic.Add(clips[i].events[j].stringParameter, clips[i].events[j].time);
                    }
                    break;
                }
            }
            return timeDic;
        }

        /// <summary>
        /// ƽ�����֣���ƽ����ķ���д������λ�ã������߲�����������
        /// </summary>
        /// <param name="go"></param>
        public void ApplyProcessingFotOutLine(GameObject go, List<string> names = null)
        {
            try
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    var child = go.transform.GetChild(i).gameObject;
                    ApplyProcessingFotOutLine(child, names);
                }

                var needApply = false;
                if (null == names)
                {
                    needApply = true;
                }
                else
                {
                    foreach (var v in names)
                    {
                        needApply = go.name.Contains(v);
                        if (needApply)
                            break;
                    }
                }

                SkinnedMeshRenderer meshRenderer = null;
                if (go.TryGetComponent<SkinnedMeshRenderer>(out meshRenderer))
                {
                    if (needApply)
                    {
                        ReadAverageNormalToTangent(meshRenderer.sharedMesh);
                    }
                    else
                    {
                        meshRenderer.material.SetFloat("_Outline", 0);
                    }
                    return;
                }

                MeshFilter meshFilter = null;
                if (go.TryGetComponent<MeshFilter>(out meshFilter))
                {
                    if (needApply)
                    {
                        ReadAverageNormalToTangent(meshFilter.sharedMesh);
                    }
                    else
                    {
                        go.GetComponent<MeshRenderer>().material.SetFloat("_Outline", 0);
                    }
                    return;
                }
            }
            catch
            {
                DebugManager.Instance.Log("����ģ�ͣ�" + go.name + "�Ƿ��Ѿ��ɼ���߲���");
            }
        }

        public static void ReadAverageNormalToTangent(Mesh mesh)
        {
            var bytes = File.ReadAllBytes(Application.dataPath + "/Textures/MeshTagentTex/" + mesh.name + ".png");
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            Vector4[] tangents = new Vector4[mesh.vertices.Length];
            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    var index = i * texture.width + j;
                    if (index >= mesh.vertices.Length)
                        break;
                    var color = texture.GetPixel(i, j);
                    //Debug.Log("Read"+color.r +"_"+ color.g+"_"+ color.b+"_"+ color.a);
                    tangents[index] = new Vector4(color.r * 2 - 1, color.g * 2 - 1, color.b * 2 - 1, color.a * 2 - 1);
                }
            }
            mesh.tangents = tangents;
        }
    }
}
