Shader "Custom/FisheyeShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Strength("Strength", Float) = 0.5
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float _Strength;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 center = float2(0.5, 0.5);
                    float2 delta = i.uv - center;
                    float distanceFromCenter = length(delta);
                    float normalizedDistance = distanceFromCenter / center.y; // 使用 center.y 以确保收缩效果在地平线以下
                    float2 newUV = center + delta * normalizedDistance * _Strength;
                    fixed4 color = tex2D(_MainTex, newUV);
                    return color;
                }
                ENDCG
            }
        }
}
