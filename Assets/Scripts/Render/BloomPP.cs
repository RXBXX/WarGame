using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] // 可在编辑模式
[ImageEffectAllowedInSceneView]
public class BloomPP : MonoBehaviour
{
    [SerializeField] Shader bloomShader;
    [SerializeField] Material mat;
    [SerializeField, Range(0, 3)] public float threshold = 0.8f;
    [SerializeField, Range(0, 7)] public int blurTime = 5;

    const string shaderPath = "PostProcess/Bloom";

    private void Awake()
    {
        bloomShader = Shader.Find(shaderPath);
        mat = new Material(bloomShader);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        mat.SetFloat("_Threshold", threshold);
        mat.SetTexture("_SourceTex", source);
        // 模糊图像
        int width = source.width;
        int height = source.height;
        // 新建tmp
        RenderTexture tmpSource, tmpDest;
        tmpSource = RenderTexture.GetTemporary(width, height, 0, source.format);
        Graphics.Blit(source, tmpSource, mat, 0);
        tmpDest = tmpSource;
        RenderTexture[] textures = new RenderTexture[blurTime];
        // 进行模糊采样
        int i = 1;
        // 向下采样
        for (; i < blurTime; i++)
        {
            width /= 2;
            height /= 2;
            tmpDest = RenderTexture.GetTemporary(width, height, 0, source.format);
            // 将贴图储存在数组里
            textures[i] = tmpDest;
            Graphics.Blit(tmpSource, tmpDest, mat, 1);
            RenderTexture.ReleaseTemporary(tmpSource);
            tmpSource = tmpDest;
        }
        // 向上采样
        for (i -= 1; i > 0; i--)
        {
            // 直接取数组里的图片赋值给tmpDest
            RenderTexture.ReleaseTemporary(tmpDest);
            tmpDest = textures[i];
            textures[i] = null;
            Graphics.Blit(tmpSource, tmpDest, mat, 2);
            RenderTexture.ReleaseTemporary(tmpSource);
            tmpSource = tmpDest;
        }
        // 传出
        Graphics.Blit(tmpSource, destination, mat, 3);
    }
}
