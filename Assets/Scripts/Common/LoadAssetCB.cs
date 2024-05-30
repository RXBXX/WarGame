using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace WarGame
{
    public delegate void LoadAssetCB<T>(T obj) where T : Object;

    public delegate void LoadSceneCB(SceneInstance obj);

    public delegate void WGArgsCallback(params object[] args);

    public delegate void WGConfigCallback(Config config);

    public struct AttrStruct
    {
        public string name;
        public string desc;
        public AttrStruct(string name, string desc)
        {
            this.name = name;
            this.desc = desc;
        }
    }
}
