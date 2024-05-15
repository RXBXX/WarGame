// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//���Shader  
//by��puppet_master  
//2017.1.5  

Shader "Custom/ToonShader"
{
    Properties
    {
       _MainTex("Texture", 2D) = "white" {}
        _Outline("Outline", Range(0,1)) = 0.1
        _OutlineColor("Outline Color", Color) = (0,0,0,0)
    }
        SubShader
    {
        Pass
        {
            Stencil //ģ���������
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"  

            sampler2D _MainTex;
            float4 _MainTex_ST;

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
            };

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
                return col;
            }
            ENDCG
        }
        Pass
        {
            Stencil //ģ���������
            {
                Ref 1
                Comp NotEqual
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag     

        #include "UnityCG.cginc"  

            float _Outline;
            fixed4 _OutlineColor;

            struct appdata
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
                float3 tangent : TANGENT; //�������߲�ֵ��
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float4 pos = UnityObjectToClipPos(v.pos);
                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.tangent.xyz);
                float3 ndcNormal = normalize(TransformViewToProjection(viewNormal.xyz)) * pos.w;//�����߱任��NDC�ռ�
                float aspect = _ScreenParams.y / _ScreenParams.x; //������Ļ�����,_ScreenParamsΪunity���ñ���
                ndcNormal.x *= aspect; //���еȱ�����
                pos.xy += 0.1 * _Outline * ndcNormal.xy;
                o.pos = pos;

                return o;
            }


            float4 frag(v2f i) : SV_TARGET
            {
                return float4(_OutlineColor.rgb, 1);
            }
            ENDCG
        }
    }
}