Shader "Custom/MyOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {} //渲染纹理
        _EdgeColor("Edge Color", color) = (0,0,0,1) //描边颜色
        _EdgeThreshold("EdgeThreshold", float) = 0.1 //描边阈值
        _EdgeWidth("_EdgeWidth", int) = 2
    }
        SubShader
        {
            CGINCLUDE

            #include "UnityCG.cginc"
            sampler2D _MainTex;

            uniform half4 _MainTex_TexelSize;

            fixed4 _EdgeColor;
            half _EdgeThreshold;
            half _EdgeOnly;
            int _EdgeWidth;

            struct v2f
            {
                float4 pos : SV_POSITION;
                half2 uv[9] : TEXCOORD0; //声明Sobel算子插值器
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                half2 uv = v.texcoord;

                //定义周围像素的uv值
                o.uv[0] = uv + _MainTex_TexelSize.xy * half2(-_EdgeWidth,-_EdgeWidth);
                o.uv[1] = uv + _MainTex_TexelSize.xy * half2(0,-_EdgeWidth);
                o.uv[2] = uv + _MainTex_TexelSize.xy * half2(_EdgeWidth,-_EdgeWidth);
                o.uv[3] = uv + _MainTex_TexelSize.xy * half2(-_EdgeWidth,0);
                o.uv[4] = uv + _MainTex_TexelSize.xy * half2(0,0);
                o.uv[5] = uv + _MainTex_TexelSize.xy * half2(_EdgeWidth,0);
                o.uv[6] = uv + _MainTex_TexelSize.xy * half2(-_EdgeWidth,_EdgeWidth);
                o.uv[7] = uv + _MainTex_TexelSize.xy * half2(0,_EdgeWidth);
                o.uv[8] = uv + _MainTex_TexelSize.xy * half2(_EdgeWidth,_EdgeWidth);

                return o;
            }
            //计算灰度值
            fixed luminance(fixed4 color)
            {
                return 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
            }
            //使用sobel算子进行卷积计算
            half Sobel(v2f i)
            {
                //定义sobel 算子
                const half Gx[9] = {-1,  0,  1,
                                    -2,  0,  2,
                                    -1,  0,  1};
                const half Gy[9] = {-1, -2, -1,
                                    0,  0,  0,
                                    1,  2,  1};
                half texColor;
                half edgeX = 0;
                half edgeY = 0;
                for (int it = 0; it < 9; it++)
                {
                    //uv采样
                    texColor = luminance(tex2D(_MainTex, i.uv[it]));
                    //累积权重
                    edgeX += texColor * Gx[it];
                    edgeY += texColor * Gy[it];
                }
                //ref: https://zhuanlan.zhihu.com/p/532483809
                half edge = 1 - abs(edgeX) - abs(edgeY);
                return edge;
            }
            fixed4 frag(v2f i) : SV_TARGET
            {
                half edge = Sobel(i); //进行sobel卷积
                edge = saturate(edge + _EdgeThreshold); //阈值offset

                fixed4 withEdge = lerp(_EdgeColor, tex2D(_MainTex, i.uv[4]), edge);
                fixed4 onlyEdge = lerp(_EdgeColor, fixed4(1,1,1,1), edge);
                fixed4 finalCol = lerp(withEdge , onlyEdge, _EdgeOnly);
                return finalCol;
            }
            ENDCG
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                ENDCG
            }
        }
}