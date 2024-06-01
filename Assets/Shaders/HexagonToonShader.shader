// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//���Shader  
//by��puppet_master  
//2017.1.5  

Shader "Custom/HexagonToonShader"
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
		pass {
			Tags{"LightMode" = "ForwardBase"}
			//Cull Back
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
					o.normal = normalize(v.normal);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.lightDir = normalize(ObjSpaceLightDir(v.vertex));
					o.viewDir = normalize(ObjSpaceViewDir(v.vertex));

					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					o.ambient = ShadeSH9(fixed4(worldNormal, 1));

					// 计算阴影数据
					TRANSFER_SHADOW(o)
					return o;
				}
				float4 frag(v2f i) :COLOR
				{
					float diff = max(0, dot(i.normal, i.lightDir));
					float toon = floor(diff * _Steps) / _Steps;

					half3 h = normalize(i.lightDir + i.viewDir);
					float nh = max(0, dot(i.normal, h));
					float spec = pow(nh, 32.0);
					float toonSpec = floor(spec * toon * 2) / 2;
					spec = lerp(spec, toonSpec, _ToonEffect);

					fixed4 col = tex2D(_MainTex, i.uv);

					fixed shadow = SHADOW_ATTENUATION(i);

					col.rgb *= (diff + spec) * _LightColor0.rgb * shadow + i.ambient;
					return col;
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