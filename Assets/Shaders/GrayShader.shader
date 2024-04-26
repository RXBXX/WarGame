Shader "Custom/GrayShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ExclusionMap("Exclusion Map", 2D) = "white" {}
        _Brightness("Brightness", Range(0.1, 2)) = 0.5
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }

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
                float4 _MainTex_ST;
                sampler2D _ExclusionMap;
                float _Brightness;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);

                // Get exclusion value from the exclusion map
                float exclusionR = tex2D(_ExclusionMap, i.uv).r;
                float exclusionG = tex2D(_ExclusionMap, i.uv).g;
                float exclusionB = tex2D(_ExclusionMap, i.uv).b;

                // If exclusion value is 1 (or greater than 0.5), exclude the pixel
                if (exclusionR >= 0.1 && exclusionG >= 0.1 && exclusionB >= 0.1)
                {
                    return col; // Return original color if excluded
                }
                else
                {
                    // Convert to grayscale and darken
                    float gray = dot(col.rgb, float3(0.299, 0.587, 0.114)) * _Brightness;
                    return float4(gray, gray, gray, col.a);
                }
            }
            ENDCG
        }
        }
}
