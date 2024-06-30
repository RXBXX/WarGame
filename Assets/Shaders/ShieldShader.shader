Shader "Custom/ShieldShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color)= (1,1,1,1 )
        _Scale("Scale",Range(1,8)) = 1
        _Strength("Strength",Range(1,10)) = 1
    }
    SubShader
    {
        Tags
        {
                "Queue" = "Transparent+1" 
                "RenderType" = "Transparent"
                "IgnoreProjector" = "True"
                "ForceNoShadowCasting" = "True"
        }
        LOD 100

        Pass
        {
            Blend One One
            //Blend SrcAlpha OneMinusSrcAlpha
            //Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
                float4 pos: TEXCOORD2;

                UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Scale;
            float _Strength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.pos = v.vertex;
                o.normal = v.normal;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                // 计算世界空间法线向量
                float3 N = normalize(mul(i.normal,(float3x3)unity_WorldToObject));

                // 计算世界空间顶点位置
                float3 worldPos = mul(unity_ObjectToWorld, i.pos).xyz;

                // 计算从顶点指向摄像机的向量
                float3 V = normalize(_WorldSpaceCameraPos.xyz - worldPos);

                float bright = 1 - saturate(dot(N, V));
                bright = pow(bright, _Scale);

                // sample the texture
                i.uv.y += _Time.y;
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _Color.rgb;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return (col + bright*_Color) * _Strength;
            }
            ENDCG
        }
    }
}
