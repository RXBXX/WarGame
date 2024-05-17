using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class WGRenderPipeline : RenderPipeline
{
    // ʹ�ô˱��������ô��ݸ����캯������Ⱦ������Դ
    private WGRenderPipelineAsset renderPipelineAsset;
    private CameraRenderer renderer = new CameraRenderer();

    // ���캯���� ExampleRenderPipelineAsset ���ʵ����Ϊ�������
    public WGRenderPipeline(WGRenderPipelineAsset asset)
    {
        renderPipelineAsset = asset;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // �������������
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera);
        }
    }
}
