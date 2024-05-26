using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Material mat;
    public RenderTexture _depthRT;
    public RenderTexture _colorRT;

    // Start is called before the first frame update
    void Start()
    {
        //var mainCamera = this.GetComponent<Camera>();
        //var _depthCamera = mainCamera.transform.Find("DepthCamera").GetComponent<Camera>();
        //_depthCamera.gameObject.SetActive(true);
        //_depthCamera.targetTexture = _depthRT;

        //var _colorCamera = mainCamera.transform.Find("ColorCamera").GetComponent<Camera>();
        //_colorCamera.gameObject.SetActive(true);
        //_colorCamera.GetComponent<Camera>().targetTexture = _colorRT;

        mat.SetTexture("_ExclusionMap", _colorRT);
        mat.SetTexture("_ExclusionMapDepth", _depthRT);
        mat.SetFloat("_StartTime", Time.timeSinceLevelLoad);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
