using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] // ���ڱ༭ģʽ
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
        // ģ��ͼ��
        int width = source.width;
        int height = source.height;
        // �½�tmp
        RenderTexture tmpSource, tmpDest;
        tmpSource = RenderTexture.GetTemporary(width, height, 0, source.format);
        Graphics.Blit(source, tmpSource, mat, 0);
        tmpDest = tmpSource;
        RenderTexture[] textures = new RenderTexture[blurTime];
        // ����ģ������
        int i = 1;
        // ���²���
        for (; i < blurTime; i++)
        {
            width /= 2;
            height /= 2;
            tmpDest = RenderTexture.GetTemporary(width, height, 0, source.format);
            // ����ͼ������������
            textures[i] = tmpDest;
            Graphics.Blit(tmpSource, tmpDest, mat, 1);
            RenderTexture.ReleaseTemporary(tmpSource);
            tmpSource = tmpDest;
        }
        // ���ϲ���
        for (i -= 1; i > 0; i--)
        {
            // ֱ��ȡ�������ͼƬ��ֵ��tmpDest
            RenderTexture.ReleaseTemporary(tmpDest);
            tmpDest = textures[i];
            textures[i] = null;
            Graphics.Blit(tmpSource, tmpDest, mat, 2);
            RenderTexture.ReleaseTemporary(tmpSource);
            tmpSource = tmpDest;
        }
        // ����
        Graphics.Blit(tmpSource, destination, mat, 3);
    }
}
