using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections;
using System.Collections.Generic;
using WarGame;

public class Test : MonoBehaviour
{
    private GameObject obj;
    private int operation;
    private bool stop = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //StartCoroutine(Load("Assets/Prefabs/Roles/Role_10001.prefab"));
            //stop = true;
            //Addressables.Release(operation);
            //operation = AssetMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Roles/Role_10001.prefab", Callback);
            

            operation = AssetMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Roles/Role_10001.prefab", Callback);
            //AssetMgr.Instance.ReleaseAssetOperation(operation);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            stop = true;
            //Destroy(obj);
            Resources.UnloadUnusedAssets();

        }
    }

    private void Callback(GameObject go)
    {
        obj = GameObject.Instantiate<GameObject>(go);
    }

    //private IEnumerator Load(string path)
    //{
    //    operation = Addressables.LoadAssetAsync<GameObject>(path);
    //    yield return operation;
    //    if (stop)
    //        yield break;
    //    Callback(new object[] { operation.Result});
    //}

}
