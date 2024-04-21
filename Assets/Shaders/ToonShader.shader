Shader "Custom/ToonShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0.0, 0.1)) = 0.05
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 200

            Pass
            {
                ZWrite On
                ZTest LEqual
                Cull Front

            // ��Ⱦ�����ڲ�
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            // ��Ⱦ����
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                return half4(1,1,1,0);
            }
            ENDCG
        }

        Pass
        {
            ZWrite On
            ZTest LEqual
            Cull Front

                // ��Ⱦ������
                ColorMask RGB
                Blend SrcAlpha OneMinusSrcAlpha

                // �������ߵı�������Ⱦ������ʹ�ñ����޳�
                Cull Front

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

        fixed4 _OutlineColor;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                };

                struct v2f
                {
                    float4 pos : POSITION;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    return o;
                }

                half4 frag(v2f i) : COLOR
                {
                    return _OutlineColor;
                }
                ENDCG
            }
        }

            FallBack "Diffuse"
}
