using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace WarGame
{
    public class AssetMgr : Singeton<AssetMgr>
    {
        private class LoadHandle
        {
            public int id;
            public AsyncOperationHandle<Object> handle;
            public Coroutine coroutine;

            public LoadHandle(int id, AsyncOperationHandle<Object> handle, Coroutine coroutine)
            {
                this.id = id;
                this.handle = handle;
                this.coroutine = coroutine;
            }
        }

        private int _id = 0;

        private Dictionary<int, LoadHandle> _operationDic = new Dictionary<int, LoadHandle>();


        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public int LoadAssetAsync<T>(string path, LoadAssetCB<T> callback) where T:Object
        {
            if (!Application.isPlaying)
            {
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                callback((T)obj);
                return 0;
            }

            _id += 1;
            var handle = Addressables.LoadAssetAsync<Object>(path);
            var coroutine = CoroutineMgr.Instance.StartCoroutine(Load<T>(handle, callback));
            _operationDic[_id] = new LoadHandle(_id, handle, coroutine);
            return _id;
        }

        /// <summary>
        /// 清理引用
        /// </summary>
        /// <param name="handle"></param>
        public void ReleaseAsset(int id)
        {
            if (!_operationDic.ContainsKey(id))
                return;

            var loadHandle = _operationDic[id];
            CoroutineMgr.Instance.StopCoroutine(loadHandle.coroutine);
            Addressables.Release(loadHandle.handle);
            _operationDic.Remove(id);
        }

        private IEnumerator Load<T>(AsyncOperationHandle<Object> handle, LoadAssetCB<T> callback) where T: Object
        {
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                callback((T)handle.Result);
            }
        }

        public float GetLoadingProgress(int id)
        {
            if (!_operationDic.ContainsKey(id))
                return 0;

            var loadHandle = _operationDic[id];
            return loadHandle.handle.PercentComplete;
        }

        public AsyncOperationHandle LoadSceneAsync(string scene, System.Action<string> callback)
        {
            var handle = Addressables.LoadSceneAsync(scene);
            handle.Completed += (AsyncOperationHandle<SceneInstance> scene) => {
                callback(scene.Result.Scene.name);
            };
            return handle;
        }

        public void ReleaseAssetOperation(AsyncOperationHandle handle)
        {
            if (handle.IsValid())
                return;

            Addressables.UnloadSceneAsync(handle);
        }

        public void Destroy(GameObject go)
        {
            GameObject.Destroy(go);
        }

        public void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        public override bool Dispose()
        {
            UnloadUnusedAssets();
            return true;
        }
    }
}