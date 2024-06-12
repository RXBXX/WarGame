Shader "Unlit/FogShader"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_FogColor("FogColor", Color) = (1,1,1,1)
			_Density("Density", Float) = 0.05
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			fixed4 _FogColor;
		float _Density;
			sampler2D _CameraDepthTexture;

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
				float4 worldPos:TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, o.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
					float depth = tex2D(_CameraDepthTexture, i.uv).r;
			//depth = depth + 0.5F;
#if defined(UNITY_REVERSED_Z)
					 depth = 1.0f - depth;
#endif
					 // 计算雾效因子
					 float fogFactor = depth;
					 fogFactor = depth * depth;// _Density* fogFactor;// exp2(-_Density * fogFactor * fogFactor * 1.442695);  // 使用平方指数雾

					 fixed4 col = tex2D(_MainTex, i.uv);
					 col.rgb = lerp(col.rgb, _FogColor.rgb, fogFactor);

					 return col;
					// fixed4 col = tex2D(_MainTex, i.uv);// *_FogColor* depth;
					//UNITY_APPLY_FOG(i.fogCoord, col);
					//return fixed4(col.xyz * (1 - depth) + _FogColor.xyz * depth, 1);
				}
				ENDCG
			}
	}
}
