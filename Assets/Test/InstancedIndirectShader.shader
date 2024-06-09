Shader "Custom/InstancedIndirectShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }
        SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                uint id : SV_InstanceID; // Instance ID

                //UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                //UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 _Color;

            StructuredBuffer<float3> _PositionBuffer; // Position buffer

            v2f vert(appdata v)
            {
                v2f o;

                // 4. setup
                //UNITY_SETUP_INSTANCE_ID(v);
                //// 在 fragment 中使用，需要在此进行设置
                //UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 position = _PositionBuffer[v.id]; // Read position from buffer
                o.vertex = UnityObjectToClipPos(v.vertex +float4(position, 0.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 4. setup
//UNITY_SETUP_INSTANCE_ID(i);
                return _Color;
            }
            ENDCG
        }
    }
}

