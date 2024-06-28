using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
//using System.Security.Cryptography;

namespace WarGame
{
    public class Tool : Singeton<Tool>
    {
        private static List<string> _appliedProcessingFotOutLine = new List<string>();

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

        public T DeserializeObject<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }

        public string SerializeObject<T>(T t)
        {
            return JsonConvert.SerializeObject(t, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
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
            return DeserializeObject<T>(jsonStr);
        }

        /// <summary>
        /// 写入json文件
        /// </summary>
        public void WriteJson<T>(string path, T t)
        {
            var jsonStr = SerializeObject<T>(t);
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
                        if (Application.isPlaying)
                        {
                            meshRenderer.material.SetFloat("_Outline", 0);
                        }
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
                        if (Application.isPlaying)
                        {
                            go.GetComponent<MeshRenderer>().material.SetFloat("_Outline", 0);
                        }
                    }
                    return;
                }
            }
            catch
            {
                DebugManager.Instance.Log("请检查模型：" + go.name + "是否已经采集描边采样||采样贴图是否设置为可读");
            }
        }

        public static void ReadAverageNormalToTangent(Mesh mesh)
        {
            if (Application.isPlaying)
            {
                if (_appliedProcessingFotOutLine.Contains(mesh.name))
                    return;
            }

            _appliedProcessingFotOutLine.Add(mesh.name);

            int assetID = 0;
            assetID = AssetsMgr.Instance.LoadAssetAsync<Texture2D>("Assets/Textures/MeshTagentTex/" + mesh.name + ".png", (texture) =>
            {
                Vector4[] tangents = new Vector4[mesh.vertices.Length];
                for (int i = 0; i < texture.width; i++)
                {
                    for (int j = 0; j < texture.height; j++)
                    {
                        var index = i * texture.width + j;
                        if (index >= mesh.vertices.Length)
                            break;
                        var color = texture.GetPixel(i, j);
                        tangents[index] = new Vector4(color.r * 2 - 1, color.g * 2 - 1, color.b * 2 - 1, color.a * 2 - 1);
                    }
                }

                AssetsMgr.Instance.ReleaseAsset(assetID);
                mesh.tangents = tangents;


                if (!Application.isPlaying)
                    DebugManager.Instance.Log("成功应用切线数据：" + mesh.name);
            });
            //var bytes = File.ReadAllBytes(Application.dataPath + "/Textures/MeshTagentTex/" + mesh.name + ".png");
            //Texture2D texture = new Texture2D(2, 2);
            //texture.LoadImage(bytes);
            //Vector4[] tangents = new Vector4[mesh.vertices.Length];
            //for (int i = 0; i < texture.width; i++)
            //{
            //    for (int j = 0; j < texture.height; j++)
            //    {
            //        var index = i * texture.width + j;
            //        if (index >= mesh.vertices.Length)
            //            break;
            //        var color = texture.GetPixel(i, j);
            //        tangents[index] = new Vector4(color.r * 2 - 1, color.g * 2 - 1, color.b * 2 - 1, color.a * 2 - 1);
            //    }
            //}

            //AssetMgr.Instance.Destroy<Texture2D>(texture);

            //mesh.tangents = tangents;

            //_appliedProcessingFotOutLine.Add(mesh.name);

            //if (!Application.isPlaying)
            //    DebugManager.Instance.Log("成功应用切线数据：" + mesh.name);
        }

        public static void ClearAppliedProcessingFotOutLine()
        {
            _appliedProcessingFotOutLine.Clear();
        }

        public static void SetLayer(Transform tran, Enum.Layer layer)
        {
            var childCount = tran.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SetLayer(tran.GetChild(i), layer);
            }
            tran.gameObject.layer = (int)layer;
        }

        /// <summary>
        /// 获取点到支线的距离
        /// </summary>
        /// <param name="linePoint"></param>
        /// <param name="lintDir"></param>
        /// <param name="point"></param>
        public static float GetDistancePointToLine(Vector3 linePoint, Vector3 lineDir, Vector3 point)
        {
            Vector3 pointToLinePoint = point - linePoint;
            float angle = Vector3.Angle(lineDir.normalized, pointToLinePoint.normalized);
            float dis = pointToLinePoint.magnitude * Mathf.Sin(angle / 180 * Mathf.PI);
            return dis;
        }

        /// <summary>
        /// 平滑发现，将平滑后的法线写入切线位置，解决描边不连续的问题
        /// </summary>
        /// <param name="go"></param>
        public void SetAlpha(GameObject go, float alpha)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                SetAlpha(child, alpha);
            }

            SkinnedMeshRenderer skinnedMR = null;
            if (go.TryGetComponent<SkinnedMeshRenderer>(out skinnedMR))
            {
                skinnedMR.material.SetFloat("_Alpha", alpha);
                return;
            }

            MeshRenderer MR = null;
            if (go.TryGetComponent<MeshRenderer>(out MR))
            {
                MR.material.SetFloat("_Alpha", alpha);
                return;
            }
        }
    }
}
