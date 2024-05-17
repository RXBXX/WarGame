Shader "PostProcess/Bloom"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Threshold("Thresold", Range(0, 1)) = 0.8
        _SourceTex("Source Texture", 2D) = "white" {}
    }
        SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass // 0 提取亮度
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

            float _Threshold;
            sampler2D _MainTex;

            half3 PreFilter(half3 c)
            {
                half brightness = max(c.r, max(c.g, c.b));
                half contribution = max(0, brightness - _Threshold);
                contribution /= max(brightness, 0.00001);
                return c * contribution;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(PreFilter(tex2D(_MainTex, i.uv)), 1);
            }
            ENDCG
        }
        Pass // 1 盒体模糊
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
            float4 _MainTex_TexelSize;

            half3 BoxFliter(float2 uv, float t)
            {
                half2 upL, upR, downL, downR;
                // 计算偏移量
                upL = _MainTex_TexelSize.xy * half2(t, 0);
                upR = _MainTex_TexelSize.xy * half2(0, t);
                downL = _MainTex_TexelSize.xy * half2(-t, 0);
                downR = _MainTex_TexelSize.xy * half2(0, -t);

                half3 col = 0;
                // 平均盒体采样
                col += tex2D(_MainTex, uv + upL).rgb * 0.25;
                col += tex2D(_MainTex, uv + upR).rgb * 0.25;
                col += tex2D(_MainTex, uv + downL).rgb * 0.25;
                col += tex2D(_MainTex, uv + downR).rgb * 0.25;

                return col;
            }


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(BoxFliter(i.uv, 1).rgb, 1);
            }
            ENDCG
        }
        Pass // 2 盒体模糊 + 亮度累加
        {
            blend one one
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
            float4 _MainTex_TexelSize;

            half3 BoxFliter(float2 uv, float t)
            {
                half2 upL, upR, downL, downR;
                // 计算偏移量
                upL = _MainTex_TexelSize.xy * half2(t, 0);
                upR = _MainTex_TexelSize.xy * half2(0, t);
                downL = _MainTex_TexelSize.xy * half2(-t, 0);
                downR = _MainTex_TexelSize.xy * half2(0, -t);

                half3 col = 0;
                // 平均盒体采样
                col += tex2D(_MainTex, uv + upL).rgb * 0.25;
                col += tex2D(_MainTex, uv + upR).rgb * 0.25;
                col += tex2D(_MainTex, uv + downL).rgb * 0.25;
                col += tex2D(_MainTex, uv + downR).rgb * 0.25;

                return col;
            }


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(BoxFliter(i.uv, 1).rgb, 1);
            }
            ENDCG
        }

        Pass // 3 合并
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

            sampler2D _SourceTex;
            sampler2D _MainTex;

             v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed3 source = tex2D(_SourceTex, i.uv).rgb;
                fixed3 blur = tex2D(_MainTex, i.uv).rgb;
                return fixed4(source + blur, 1);
            }

            ENDCG
        }
    }
}


作者: 正式这种素质
链接 : https://1keven1.github.io/2021/03/16/%E3%80%90Unity%E3%80%91%E5%90%8E%E6%9C%9F%E5%A4%84%E7%90%86Bloom%E6%95%88%E6%9E%9C/
来源: SuzhiのBlog
著作权归作者所有。商业转载请联系作者获得授权，非商业转载请注明出处。