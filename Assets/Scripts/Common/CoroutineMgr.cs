using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class CoroutineBehaviour : MonoBehaviour
    { }

    public class CoroutineMgr : Singeton<CoroutineMgr>
    {
        private MonoBehaviour _monoBehaviour;

        public override bool Init()
        {
            base.Init();

            GameObject go = new GameObject();
            GameObject.DontDestroyOnLoad(go);
            _monoBehaviour = go.AddComponent<CoroutineBehaviour>();
            //_monoBehaviour = new MonoBehaviour(); //MonoBehaviour不允许通过new实例化
            return true;
        }

        public Coroutine StartCoroutine(IEnumerator ie)
        {
            if (null == _monoBehaviour)
                Init();
            return _monoBehaviour.StartCoroutine(ie);
        }

        public void StopCoroutine(Coroutine ie)
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
