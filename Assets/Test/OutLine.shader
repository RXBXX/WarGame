Shader "Unlit/StentilOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Stencil {
             Ref 0          //0-255
             Comp Equal     //default:always
             Pass IncrSat   //default:keep
             Fail keep      //default:keep
             ZFail keep     //default:keep
        }

        Pass
        {
            Tags{"LightMode" = "ForwardBase"}
            Cull Back
                ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
        // ����ɫ������ɶ������Ӱ��û����Ӱ�ı���
        //�����ǻ��������κι�����ͼ������������Щ���壩
        #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
        // ��Ӱ helper �����ͺ�
        #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
            // apply fog
            UNITY_APPLY_FOG(i.fogCoord, col);
            //return fixed4(1,1,0,1);
            return col;
        }
        ENDCG
    }

    Pass
    {
Cull Front
                CGPROGRAM
            // Apparently need to add this declaration
            #pragma multi_compile_fwdadd_fullshadows
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float4 normal: NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = v.vertex + normalize(v.normal) * 0.1f;
            o.vertex = UnityObjectToClipPos(o.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            UNITY_TRANSFER_FOG(o,o.vertex);
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            // sample the texture
            fixed4 col = tex2D(_MainTex, i.uv);
        // apply fog
           UNITY_APPLY_FOG(i.fogCoord, col);
        return fixed4(1,1,1,1);
    }
    ENDCG
}
    }
}

