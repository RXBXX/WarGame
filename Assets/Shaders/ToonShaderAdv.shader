Shader "Custom/ToonShaderAdv"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Brightness("Brightness", Range(0.1, 2)) = 0.5
        _ExclusionMap("Exclusion Map", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", Range(0.0, 0.1)) = 0.01
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
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float3 normal : TEXCOORD1;
                };

                sampler2D _MainTex;
                sampler2D _ExclusionMap;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _Brightness;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.normal = UnityObjectToWorldNormal(v.normal);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Get exclusion value from the exclusion map
                    float exclusion = tex2D(_ExclusionMap, i.uv).r;

                // If exclusion value is 1 (or greater than 0.5), exclude the pixel
                if (exclusion >= 0.1)
                {
                    // Return original color if excluded
                    return tex2D(_MainTex, i.uv);
                }
                else
                {
                    // Toon shading
                    float dotNL = dot(normalize(i.normal), float3(0, 0, 1));
                    float threshold = 0.5;
                    float3 color = dotNL > threshold ? float3(1, 1, 1) : float3(0, 0, 0);

                    // Apply outline
                    float outline = fwidth(dotNL);
                    float edge = saturate(outline - _OutlineWidth);
                    color *= edge;

                    // Convert to grayscale and adjust brightness
                    float gray = dot(color, float3(0.299, 0.587, 0.114)) * _Brightness;

                    // Combine with original color
                    return lerp(float4(gray, gray, gray, 1), tex2D(_MainTex, i.uv), 0.5);
                }
            }
            ENDCG
        }
        }
}
