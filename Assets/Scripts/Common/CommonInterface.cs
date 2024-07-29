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
            get { return new Vector3(0, 0.2F, 0); }
        }

        public static Vector3 RoleOffset
        {
            get { return new Vector3(0f, 0.22f, 0f); }
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

        public static Color GetPalette(Enum.Element element)
        {
            switch (element)
            {
                case Enum.Element.Earth:
                    return new Color(1.0F, 0.9257F, 0.8504f, 1.0f);
                case Enum.Element.Fire:
                    return new Color(1.0F, 0.7857F, 0.7954f, 1.0f);
                case Enum.Element.Metal:
                    return new Color(1.0F, 0.8991F, 0.7803f, 1.0f);
                case Enum.Element.Water:
                    return new Color(0.9097F, 0.9595F, 1.0f, 1.0f);
                case Enum.Element.Wood:
                    return new Color(0.9047F, 0.9583F, 0.9946f, 1.0f);
                default:
                    return Color.white;
            }
        }
    }

    [Serializable]
    public struct WGVector3
    {
        public float x;
        public float y;
        public float z;

        public WGVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static WGVector3 operator +(WGVector3 v1, WGVector3 v2)
        {
            return new WGVector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static bool operator ==(WGVector3 v1, WGVector3 v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public static bool operator !=(WGVector3 v1, WGVector3 v2)
        {
            return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
        }

        public override bool Equals(object v)
        {
            return this == (WGVector3)v;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static float Distance(WGVector3 v1, WGVector3 v2)
        {
            return Mathf.Sqrt(Mathf.Pow(v2.x - v1.x, 2) + Mathf.Pow(v2.y - v1.y, 2) + Mathf.Pow(v2.z - v1.z, 2));
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static WGVector3 ToWGVector3(Vector3 vec)
        {
            return new WGVector3(vec.x, vec.y, vec.z);
        }
    }

    //[Serializable]
    //public struct WGQuaternion
    //{
    //    public float x;
    //    public float y;
    //    public float z;
    //    public float w;
    //    public WGQuaternion(float x, float y, float z, float w)
    //    {
    //        this.x = x;
    //        this.y = y;
    //        this.z = z;
    //        this.w = w;
    //    }
    //}
}