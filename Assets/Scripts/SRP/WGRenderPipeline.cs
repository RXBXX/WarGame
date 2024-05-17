using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class WGRenderPipeline : RenderPipeline
{
    // 使用此变量来引用传递给构造函数的渲染管线资源
    private WGRenderPipelineAsset renderPipelineAsset;
    private CameraRenderer renderer = new CameraRenderer();

    // 构造函数将 ExampleRenderPipelineAsset 类的实例作为其参数。
    public WGRenderPipeline(WGRenderPipelineAsset asset)
    {
        renderPipelineAsset = asset;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // 遍历所有摄像机
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera);
        }
    }
}
