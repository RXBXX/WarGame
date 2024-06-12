using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMesh : MonoBehaviour
{
    public Texture2D tex1;
    public Texture2D tex2;
    public Material Mat;
    public Mesh mesh;
    private Matrix4x4[] _matrixs;
    private MaterialPropertyBlock block;
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        block = new MaterialPropertyBlock();
        _matrixs = new Matrix4x4[10 * 10];
        var colors = new float[100];
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                var tex = i % 2 == 0 ? tex1 : tex2;
                //blocks[i * 10 + j] = new MaterialPropertyBlock();
                //Debug.Log(blocks[i * 10 + j].GetColor("_Color"));
                colors[i * 10 + j] = (i * 10 + j)%2;
                _matrixs[i * 10 + j] = Matrix4x4.TRS(new Vector3(i, 0, j), Quaternion.identity, Vector3.one);
            }
        }
        block.SetFloatArray("_TexIndex", colors);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(blocks[99].GetColor("_Color"));
        //for (int i = 0; i < _matrixs.Length; i++)
        //{
        //    Graphics.DrawMesh(mesh, _matrixs[i], Mat, 0, null, 0, blocks[i]);
        //}
        //Graphics.DrawMeshInstanced(mesh, 0, Mat, _matrixs, _matrixs.Length, block);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, Mat);
    }
}
