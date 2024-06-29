using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using System;

namespace WarGame
{
    public delegate void LoadAssetCB<T>(T obj) where T : UnityEngine.Object;

    public delegate void LoadSceneCB(SceneInstance obj);

    public delegate void WGArgsCallback(params object[] args);

    public delegate void WGConfigCallback<T>(T config) where T : Config;

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

    [Serializable]
    public struct IntFloatPair
    {
        public int id;
        public float value;

        public IntFloatPair(int id, float value)
        {
            this.id = id;
            this.value = value;
        }
    }

    [Serializable]
    public struct TwoIntPair
    {
        public int id;
        public int value;

        public TwoIntPair(int id, int value)
        {
            this.id = id;
            this.value = value;
        }
    }

    [Serializable]
    public struct BuffPair
    {
        public int id;
        public int value;
        public Enum.RoleType initiatorType;

        public BuffPair(int id, int value, Enum.RoleType initiatorType)
        {
            this.id = id;
            this.value = value;
            this.initiatorType = initiatorType;
        }
    }

    [Serializable]
    public struct TwoStrPair
    {
        public string id;
        public string value;

        public TwoStrPair(string id, string value)
        {
            this.id = id;
            this.value = value;
        }
    }

    public struct ThreeStrPair
    {
        public string name;
        public string value1;
        public string value2;
        public ThreeStrPair(string name, string value1, string value2)
        {
            this.name = name;
            this.value1 = value1;
            this.value2 = value2;
        }
    }

    public struct FourStrPair
    {
        public string name;
        public string value1;
        public string value2;
        public string value3;

        public FourStrPair(string name, string value1, string value2, string value3)
        {
            this.name = name;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
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

        public static Color GetElementColor(Enum.Element element)
        {
            switch (element)
            {
                case Enum.Element.Metal:
                    return new Color(1, 0.9378F, 0);
                case Enum.Element.Wood:
                    return new Color(0, 1, 0.0638F);
                case Enum.Element.Water:
                    return new Color(0, 0.8846F, 1);
                case Enum.Element.Fire:
                    return new Color(1, 0.8150F, 0);
                case Enum.Element.Earth:
                    return new Color(0.7529F, 0.4941F, 0.0588F);
                default:
                    return Color.white;
            }
        }
    }
}