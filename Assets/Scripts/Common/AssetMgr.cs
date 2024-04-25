using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WarGame
{
    public class AssetMgr : Singeton<AssetMgr>
    {
        public T LoadAsset<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}