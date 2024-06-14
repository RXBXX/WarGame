using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace WarGame
{
    public delegate void LoadAssetCB<T>(T obj) where T : Object;

    public delegate void LoadSceneCB(SceneInstance obj);

    public delegate void WGArgsCallback(params object[] args);

    public delegate void WGConfigCallback<T>(T config) where T:Config;

    public delegate void WGCallback();

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

    public delegate void BattleRoundFunc();

    public struct StringCallbackStruct
    {
        public string title;
        public WGCallback callback;

        public StringCallbackStruct(string title, WGCallback callback)
        {
            this.title = title;
            this.callback = callback;
        }
    }

    public class CommonParams
    {
        public static Vector3 Offset
        {
            get { return new Vector3(0, 0.224F, 0); }
        }

        public static string GetAttrColor(Enum.AttrType attrType)
        {
            switch (attrType)
            {
                case Enum.AttrType.HP:
                    return "#CE4A35";
                case Enum.AttrType.Rage:
                    return "#FFB500";
                default:
                    return "#5c8799";
            }
        }
    }
}