Shader "Custom/BlurShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BlurSize("Blur Size", Range(0, 0.1)) = 0.0001
        _BlurStep("BlurStep", Range(4, 20)) = 5
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
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
                float4 _MainTex_ST;
                float _BlurSize;
                float _BlurStep;
                uniform half4 _MainTex_TexelSize;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.uv;
                    float2 offset = _BlurSize * _MainTex_TexelSize.zw;
                    float iterNum = 0;
                    fixed4 col = tex2D(_MainTex, uv);
                    iterNum++;
                    for (int q = - offset.x; q < offset.x; q+=_BlurStep)
                    {
                        float uvPosX = uv.x + q * _MainTex_TexelSize.x;
                        if (uvPosX <= 0)
                            uvPosX = 0;
                        if (uvPosX >= 1)
                            uvPosX = 1;
                        for (int j = -offset.y; j < offset.y; j+=_BlurStep)
                            {
                                float uvPosY = uv.y + j * _MainTex_TexelSize.y;
                                if (uvPosY <= 0)
                                    uvPosY = 0;
                                if (uvPosY >= 1)
                                    uvPosY = 1;
                                col += tex2D(_MainTex, float2(uvPosX, uvPosY));
                                iterNum++;
                            }
                    }
                    col /= iterNum;
                    //float iterNum = (offset.x * 2) * (offset.y * 2);
                    //return fixed4(iterNum / 255000, 0, 0, 1);
                    return col;
                }
                ENDCG
            }
        }
}
