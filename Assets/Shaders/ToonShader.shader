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
		_Factor("Factor",range(0,1)) = 0.5//������Զ
		_ToonEffect("Toon Effect",range(0,1)) = 0.5//��ͨ���̶ȣ�����Ԫ������Ԫ�Ľ����ߣ�
		_Steps("Steps of toon",range(0,9)) = 3//ɫ�ײ���
		_Color("Color", Color) = (0, 0, 0, 0)
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

			//float4 _LightColor0;
			float4 _Color;
			float _Steps;
			float _ToonEffect;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos:SV_POSITION;
				float3 lightDir:TEXCOORD0;
				float3 viewDir:TEXCOORD1;
				float3 normal:TEXCOORD2;
				float2 uv : TEXCOORD3;
				float3 ambient:TEXCOORD4;
				SHADOW_COORDS(5) // 将阴影数据放入 TEXCOORD1
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);//�л�����������
				o.normal = v.normal;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.viewDir = ObjSpaceViewDir(v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.ambient = ShadeSH9(fixed4(worldNormal, 1));
				// 计算阴影数据
				TRANSFER_SHADOW(o)
				return o;
			}
			float4 frag(v2f i) :COLOR
			{
				float3 N = normalize(i.normal);
				float3 viewDir = normalize(i.viewDir);
				float3 lightDir = normalize(i.lightDir);

				float diff = max(0,dot(N,i.lightDir));
				diff = (diff + 1) / 2;
				diff = smoothstep(0,1,diff);
				float toon = floor(diff * _Steps) / _Steps;
				diff = lerp(diff,toon,_ToonEffect);

				half3 h = normalize(lightDir + viewDir);
				float nh = max(0, dot(N, h));
				float spec = pow(nh, 32.0);
				float toonSpec = floor(spec * toon * 2) / 2;
				spec = lerp(spec, toonSpec, _ToonEffect);

				fixed4 col = tex2D(_MainTex, i.uv);

				fixed shadow = SHADOW_ATTENUATION(i);

				float4 diffCol = (_LightColor0 * (diff + spec)* shadow);
				diffCol.rgb += i.ambient;
				return col * diffCol * _Color;
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