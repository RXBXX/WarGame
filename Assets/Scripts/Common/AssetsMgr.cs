using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarGame
{
    public class AssetPair<T> where T:Object
    {
        public int ID;
        public T Obj;

        public void Dispose()
        {
            if (null != Obj)
            {
                AssetsMgr.Instance.Destroy(Obj);
                Obj = null;
            }
            if (0 != ID)
            {
                AssetsMgr.Instance.ReleaseAsset(ID);
                ID = 0;
            }
        }
    }

    public class AssetsMgr : Singeton<AssetsMgr>
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

            public void Dispose()
            {
                if (null != coroutine)
                    CoroutineMgr.Instance.StopCoroutine(coroutine);
                Addressables.Release(handle);
            }
        }

        private int _id = 0;

        private Dictionary<int, LoadHandle> _operationDic = new Dictionary<int, LoadHandle>();


        //        /// <summary>
        //        /// 加载资源
        //        /// </summary>
        //        /// <typeparam name="T"></typeparam>
        //        /// <param name="path"></param>
        //        /// <param name="callback"></param>
        //        /// <returns></returns>
        //        public int LoadAsset<T>(string path, ref T obj) where T : Object
        //        {
        //            _id += 1;
        //#if UNITY_EDITOR
        //            if (!Application.isPlaying)
        //            {
        //                obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        //                return _id;
        //            }
        //#endif
        //            var handle = Addressables.LoadAssetAsync<Object>(path);
        //            _operationDic[_id] = new LoadHandle(_id, handle, null);
        //            //这里在回调里调用WaitForCompletion,所以同步加载禁止掉
        //            //Reentering the Update method is not allowed.This can happen when calling WaitForCompletion on an operation while inside of a callback.
        //            handle.WaitForCompletion();
        //            if (handle.Status == AsyncOperationStatus.Succeeded)
        //            {
        //                DebugManager.Instance.Log(handle.Result.GetType());
        //                obj = (T)handle.Result;
        //                return _id;
        //            }

        //            return 0;
        //        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public int LoadAssetAsync<T>(string path, LoadAssetCB<T> callback, LoadAssetCB<T> faildCallback = null) where T : Object
        {
            _id++;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                callback((T)obj);
                return _id;
            }
#endif
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

            _operationDic[id].Dispose();
            //var loadHandle = _operationDic[id];
            //if (null != loadHandle.coroutine)
            //    CoroutineMgr.Instance.StopCoroutine(loadHandle.coroutine);
            //Addressables.Release(loadHandle.handle);
            _operationDic.Remove(id);
        }

        private IEnumerator Load<T>(AsyncOperationHandle<Object> handle, LoadAssetCB<T> callback) where T : Object
        {
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                callback((T)handle.Result);
            }
            else
            {
                DebugManager.Instance.LogError($"Failed to load addressable asset at address: , error: {handle.OperationException}");
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
            handle.Completed += (AsyncOperationHandle<SceneInstance> scene) =>
            {
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

        public void Destroy<T>(T go) where T : Object
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