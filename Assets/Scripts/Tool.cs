using UnityEngine;

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
        public string ToJson<T>(T obj)
        {
            if (typeof(T).GetInterface("IList") != null)
            {
                Pack<T> pack = new Pack<T>(obj);
                pack.data = obj;
                string json = JsonUtility.ToJson(pack);
                Debug.Log(json);
                return json.Substring(8, json.Length - 9);
            }

            return JsonUtility.ToJson(obj);
        }

        public T FromJson<T>(string json)
        {
            if (json == "null" && typeof(T).IsClass) return default(T);
            if (typeof(T).GetInterface("IList") != null)
            {
                json = "{\"data\":{data}}".Replace("{data}", json);
                Pack<T> Pack = JsonUtility.FromJson<Pack<T>>(json);
                return Pack.data;
            }
            return JsonUtility.FromJson<T>(json);
        }
    }
}
