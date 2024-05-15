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

        ///在使用Unity官方提供的JsonUtility类进行JSON转换时，发现一旦转换数组就会出现问题,需要在数组上套个壳
        ///另外发现UnityEngine.Json对Dictionary也不支持
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
        /// 读取json文件
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
        /// 写入json文件
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
        /// 平滑发现，将平滑后的法线写入切线位置，解决描边不连续的问题
        /// </summary>
        /// <param name="go"></param>
        public void PreProcessingFotOutLine(GameObject go)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                PreProcessingFotOutLine(go.transform.GetChild(i).gameObject);
            }

            SkinnedMeshRenderer meshRenderer = null;
            if (go.TryGetComponent<SkinnedMeshRenderer>(out meshRenderer))
            {
                var mesh = meshRenderer.sharedMesh;
                WriteAverageNormalToTangent(mesh);
                return;
            }

            MeshFilter meshFilter = null;
            if (go.TryGetComponent<MeshFilter>(out meshFilter))
            {
                var mesh = meshFilter.sharedMesh;
                WriteAverageNormalToTangent(mesh);
                return;
            }
        }

        private void WriteAverageNormalToTangent(Mesh mesh)
        {
            var averageNormalHash = new Dictionary<Vector3, Vector3>();
            for (var j = 0; j < mesh.vertices.Length; j++)
            {
                if (!averageNormalHash.ContainsKey(mesh.vertices[j]))
                {
                    averageNormalHash.Add(mesh.vertices[j], mesh.normals[j]);
                }
                else
                {
                    averageNormalHash[mesh.vertices[j]] = (averageNormalHash[mesh.vertices[j]] + mesh.normals[j]).normalized;
                }
            }
            var averageNormals = new Vector3[mesh.vertexCount];
            for (var j = 0; j < mesh.vertices.Length; j++)
            {
                averageNormals[j] = averageNormalHash[mesh.vertices[j]];
            }

            var tangents = new Vector4[mesh.vertexCount];
            for (var j = 0; j < mesh.vertices.Length; j++)
            {
                tangents[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z, 0);
            }
            mesh.tangents = tangents;
        }
    }
}
