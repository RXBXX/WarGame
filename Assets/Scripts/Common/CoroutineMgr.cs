using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Coroutine : MonoBehaviour
    { }

    public class CoroutineMgr : Singeton<CoroutineMgr>
    {
        private MonoBehaviour _monoBehaviour;

        public override bool Init()
        {
            base.Init();

            GameObject go = new GameObject();
            GameObject.DontDestroyOnLoad(go);
            _monoBehaviour = go.AddComponent<Coroutine>();
            //_monoBehaviour = new MonoBehaviour(); //MonoBehaviour不允许通过new实例化
            return true;
        }

        public void StartCoroutine(IEnumerator ie)
        {
            _monoBehaviour.StartCoroutine(ie);
        }

        public void StopCoroutine(IEnumerator ie)
        {
            _monoBehaviour.StopCoroutine(ie);
        }

        public void StopAllCoroutines()
        {
            _monoBehaviour.StopAllCoroutines();
        }

        public override bool Dispose()
        {
            return base.Dispose();
        }
    }
}
