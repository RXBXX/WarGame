Shader "Examples/SimpleUnlitColor"
{
    SubShader
    {
        Pass
        {
            // LightMode 通道标签的值必须与 ScriptableRenderContext.DrawRenderers 中的 ShaderTagId 匹配
            Tags { "Render" = "SRPDefaultUnlit"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

    float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionCS = mul(unity_MatrixVP, worldPos);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_TARGET
            {
                return float4(0.5,1,0.5,1);
            }
            ENDHLSL
        }
    }
}