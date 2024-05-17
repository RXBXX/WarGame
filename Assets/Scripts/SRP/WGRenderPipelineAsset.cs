using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/WGRenderPipelineAsset")]
public class WGRenderPipelineAsset : RenderPipelineAsset
{
    // ������ Inspector ��Ϊÿ����Ⱦ������Դ���������
    public Color exampleColor;
    public string exampleString;

    // Unity ����Ⱦ��һ֮֡ǰ���ô˷�����
    // �����Ⱦ������Դ�ϵ����øı䣬Unity �����ٵ�ǰ����Ⱦ����ʵ����������Ⱦ��һ֮֡ǰ�ٴε��ô˷�����
    protected override RenderPipeline CreatePipeline()
    {
        // ʵ�������Զ��� SRP ������Ⱦ����Ⱦ���ߣ�Ȼ�󴫵ݶԴ���Ⱦ������Դ�����á�
        // Ȼ����Ⱦ����ʵ�����Է����Ϸ�������������ݡ�
        return new WGRenderPipeline(this);
    }
}
