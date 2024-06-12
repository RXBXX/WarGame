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
		_HighLight("HighLight", Range(0,1)) = 1
		_ToonEffect("Toon Effect",range(0,1)) = 0.5
		_Steps("Steps of toon",range(0,9)) = 3
		_AmbientStrength("Outline", Range(0,10)) = 10
	}
	SubShader
	{
		pass {//ƽ�й�ĵ�pass��Ⱦ
			Tags{"LightMode" = "ForwardBase"}
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			// 将着色器编译成多个有阴影和没有阴影的变体
			//（我们还不关心任何光照贴图，所以跳过这些变体）
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			// 阴影 helper 函数和宏
			#include "AutoLight.cginc"

			float _Steps;
			float _ToonEffect;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _AmbientStrength;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos:SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float2 uv : TEXCOORD2;
				SHADOW_COORDS(3) // 将阴影数据放入 TEXCOORD1
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				// 计算阴影数据
				TRANSFER_SHADOW(o)
				return o;
			}
			float4 frag(v2f i) :COLOR
			{
				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _AmbientStrength;

				fixed diffuse = max(0, dot(worldNormal, worldLightDir));

				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				fixed3 halfDir = normalize(worldLightDir + viewDir);

				float nh = max(0, dot(i.worldNormal, halfDir));
				float toon = floor(max(0, diffuse) * _Steps) / _Steps;
				float specular = pow(nh, 32.0);
				float toonSpec = floor(specular * toon * 2) / 2;
				specular = lerp(specular, toonSpec, _ToonEffect);

				fixed shadow = SHADOW_ATTENUATION(i);

				fixed4 col = tex2D(_MainTex, i.uv);

				return fixed4((ambient + (diffuse + specular) * _LightColor0.rgb * shadow) * col.rgb, 1.0);
			}
			ENDCG
		}
		Pass {
			// Pass for other pixel lights
			Tags { "LightMode" = "ForwardAdd" }

			Blend One One

			CGPROGRAM

			// Apparently need to add this declaration
			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			float _Steps;
			float _ToonEffect;

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				LIGHTING_COORDS(2, 3)//包含光照衰减以及阴影
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				TRANSFER_VERTEX_TO_FRAGMENT(o);//包含光照衰减以及阴影
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed3 worldNormal = normalize(i.worldNormal);

				//判断是否是平行光，获得世界坐标下光线方向
				#ifdef USING_DIRECTIONAL_LIGHT
					fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
				#else
					fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz);
				#endif

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				fixed diffuse = max(0, dot(worldNormal, worldLightDir));

				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				fixed3 halfDir = normalize(worldLightDir + viewDir);

				float nh = max(0, dot(i.worldNormal, halfDir));
				float toon = floor(max(0, diffuse) * _Steps) / _Steps;
				float specular = pow(nh, 32.0);
				float toonSpec = floor(specular * toon * 2) / 2;
				specular = lerp(specular, toonSpec, _ToonEffect);

				////判断是否是平行光，处理衰减
				//#ifdef USING_DIRECTIONAL_LIGHT
				//	fixed atten = 1.0;
				//#else
				//	#if defined (POINT)
				//		float3 lightCoord = mul(unity_WorldToLight, float4(i.worldPos, 1)).xyz;
				//		fixed atten = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL;
				//	#elif defined (SPOT)
				//		float4 lightCoord = mul(unity_WorldToLight, float4(i.worldPos, 1));
				//		fixed atten = (lightCoord.z > 0) * tex2D(_LightTexture0, lightCoord.xy / lightCoord.w + 0.5).w * tex2D(_LightTextureB0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL;
				//	#else
				//		fixed atten = 1.0;
				//	#endif
				//#endif

				UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
				return fixed4((diffuse + specular) * _LightColor0.rgb * atten * atten, 1.0);
			}

			ENDCG
		}
		Pass
		{
			Cull Front
			//Stencil //ģ���������
			//{
				//    Ref 1
				//    Comp NotEqual
			//}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag     
			#include "UnityCG.cginc"  
			#include "UnityLightingCommon.cginc"

			float _Outline;
			fixed4 _OutlineColor;
			float _HighLight;

			struct appdata
			{
				float4 pos : POSITION;
				float3 normal : NORMAL;
				float3 tangent : TANGENT;
			};
			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 diff : COLOR0;
			};

			v2f vert(appdata v)
			{
				v2f o;

				float4 pos = UnityObjectToClipPos(v.pos);
				float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.tangent.xyz);
				float3 ndcNormal = normalize(TransformViewToProjection(viewNormal.xyz)) * pos.w;
				float aspect = _ScreenParams.y / _ScreenParams.x;
				ndcNormal.x *= aspect;
				pos.xy += 0.1 * _Outline * ndcNormal.xy;
				o.pos = pos;

				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				return o;
			}

			float4 frag(v2f i) : SV_TARGET
			{
				if (_HighLight > 0.5)
				{
					return float4(_OutlineColor.rgb, 1) * i.diff;
				}
				else
				{
					return float4(_OutlineColor.rgb, 1);
				}
			}
			ENDCG
		}

		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}