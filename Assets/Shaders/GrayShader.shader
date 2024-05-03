Shader "Custom/GrayShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	    _StartTime("StartTime", float) = 0.0
		_ExclusionMap("Exclusion Map", 2D) = "white" {}
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
				float _Brightness;
				float _StartTime;
				float _Speed;

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
				fixed4 exclusion = tex2D(_ExclusionMap, i.uv);

				// y = -x + 0.8
				float time = (_Time.y - _StartTime) * _Speed;
				if(time < 0)
					return tex2D(_MainTex, i.uv);

				float posX = i.uv.x - 0.5;
				float posY = i.uv.y - 0.5;
				float cross = - time * posY - 1.2F * time * (posX - time);
				float cross2 = time * posY + 1.2F * time * (posX + time);
				if (cross2 < 0 || cross < 0)
				{
					return tex2D(_MainTex, i.uv);
				}
				else
				{
					// If exclusion value is 1 (or greater than 0.5), exclude the pixel
					if (exclusion.r >= 0.1 && exclusion.g >= 0.1 && exclusion.b >= 0.1)
					{
						//return col; // Return original color if excluded
						return tex2D(_ExclusionMap, i.uv);
					}
					else
					{
						return dot(col.rgb, float3(0.299, 0.587, 0.114)) * _Brightness;
					}
				}
			}
			ENDCG
		}
		}
}
