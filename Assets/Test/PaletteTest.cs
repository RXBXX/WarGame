using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class PaletteTest : MonoBehaviour
{
    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ExecuteInEditMode]
    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Debug.Log("OnRenderImage");
        Graphics.Blit(source, destination, material);
    }
}
