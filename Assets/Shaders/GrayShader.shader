Shader "Custom/GrayShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_StartTime("StartTime", float) = 0.0
		_ExclusionMap("Exclusion Map", 2D) = "white" {}
		_ExclusionMapDepth("Exclusion Map Depth", 2D) = "white" {}
		_Brightness("Brightness", Range(0.1, 2)) = 0.5
		_Speed("Speed", float) = 5
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
				sampler2D _ExclusionMapDepth;
				float _Brightness;
				float _StartTime;
				float _Speed;
				sampler2D _CameraDepthTexture; // Declare _CameraDepthTexture

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
				fixed4 exclusionDepth = tex2D(_ExclusionMapDepth, i.uv);

				float time = (_Time.y - _StartTime) * _Speed;
				if (time < 0)
					return tex2D(_MainTex, i.uv);

				float posX = i.uv.x - 0.5;
				float posY = i.uv.y - 0.5;
				float cross = -time * posY - 1.2F * time * (posX - time);
				float cross2 = time * posY + 1.2F * time * (posX + time);
				if (cross2 < 0 || cross < 0)
				{
					return tex2D(_MainTex, i.uv);
				}
				else
				{
					//如果想要取到depth值，需要将摄像机的depthTextureMode = DepthTextureMode.Depth;
					float depth = tex2D(_CameraDepthTexture, i.uv).r;
					float exclusionDepthValue = exclusionDepth.x;
					//float exclusionDepthValue = tex2D(_ExclusionMapDepth, i.vertex.xy / i.vertex.w).r;
#if defined(UNITY_REVERSED_Z)
					depth = 1.0f - depth;
					exclusionDepthValue = 1.0f - exclusionDepthValue;
#endif
					fixed4 grayCol = dot(col.rgb, float3(0.299, 0.587, 0.114)) * _Brightness;
					//这里的0.0001F是因为深度值有精度误差
					if (exclusionDepthValue < 1&& exclusionDepthValue <= depth)
					{
						fixed4 exCol = tex2D(_ExclusionMap, i.uv);
						//当像素为黑色，剔除，这里是为了处理应用了叠加模式的特效，如果特效贴图有黑色部分，这里不处理的话，特效就会遮挡背景
						//这是应该使用叠加算法，目前先暂时简单处理
						if (exCol.g <= 0 && exCol.b <= 0 && exCol.r <= 0)
							exCol.a = 0;
						return exCol * exCol.a + grayCol * (1 - exCol.a);
					}
					else
					{
						return grayCol;
					}
				}
			}
			ENDCG
		}
		}
}
