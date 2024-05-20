using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace WarGame
{
    public delegate void LoadAssetCB<T>(T obj) where T : Object;

    public delegate void LoadSceneCB(SceneInstance obj);
}
