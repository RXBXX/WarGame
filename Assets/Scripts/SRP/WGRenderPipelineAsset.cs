using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/WGRenderPipelineAsset")]
public class WGRenderPipelineAsset : RenderPipelineAsset
{
    // 可以在 Inspector 中为每个渲染管线资源定义此数据
    public Color exampleColor;
    public string exampleString;

    // Unity 在渲染第一帧之前调用此方法。
    // 如果渲染管线资源上的设置改变，Unity 将销毁当前的渲染管线实例，并在渲染下一帧之前再次调用此方法。
    protected override RenderPipeline CreatePipeline()
    {
        // 实例化此自定义 SRP 用于渲染的渲染管线，然后传递对此渲染管线资源的引用。
        // 然后，渲染管线实例可以访问上方定义的配置数据。
        return new WGRenderPipeline(this);
    }
}
