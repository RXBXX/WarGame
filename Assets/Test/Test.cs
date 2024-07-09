using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections;
using System.Collections.Generic;
using WarGame;
using System.Text;

public class Test : MonoBehaviour
{
    private GameObject obj;
    private int operation;
    private bool stop = false;
    public Material mat;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UnityEngine.Profiling.Profiler.BeginSample("Str Profiler");
            for (int i = 0; i < 10000; i++)
            {
                var sb = new StringBuilder();
                sb.Append(1);
                sb.Append('_');
                sb.Append(2);
                sb.Append('_');
                sb.Append(3);
                var str = sb.ToString();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            UnityEngine.Profiling.Profiler.BeginSample("Int Profiler");
            for (int i = 0; i < 100000000; i++)
            {
                var index = ((1 * 100) + 1) * 100 + 1;
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    private void Callback(GameObject go)
    {
        obj = GameObject.Instantiate<GameObject>(go);
    }


    //public void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    Graphics.Blit(source, destination, mat);
    //}
    //private IEnumerator Load(string path)
    //{
    //    operation = Addressables.LoadAssetAsync<GameObject>(path);
    //    yield return operation;
    //    if (stop)
    //        yield break;
    //    Callback(new object[] { operation.Result});
    //}

}
