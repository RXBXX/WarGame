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
    private float _time;
    private float _Intervale = 0.1F;
    public  Animator animator;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        if (_time <= _Intervale)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            animator.SetBool("Attack", false);
            animator.SetBool("Attacked", true);
        }
        //    Debug.Log("MouseDown");
        else if (Input.GetMouseButtonDown(0))
        {
            //_time = 0;
            //Debug.Log("MouseUp");
            animator.SetBool("Idle", false);
            animator.SetBool("Attack", true);
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
