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
	}
		SubShader
		{
			Pass
			{
				Tags {"LightMode" = "ForwardBase"}
				//Stencil //ģ���������
				//{
				//    Ref 1
				//    Comp Always
				//    Pass Replace
				//}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"  
				#include "UnityLightingCommon.cginc" // ���� _LightColor0

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
					fixed4 diff : COLOR0; // ���������ɫ
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					// ������ռ��л�ȡ���㷨��
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					// ��׼���䣨�����أ����յķ��ߺ�
					// ���߷���֮��ĵ��
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					// ����ǳɫ����
					o.diff = nl * _LightColor0;
					o.diff.rgb += ShadeSH9(half4(worldNormal, 1));
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					return col * i.diff;
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
				#include "UnityLightingCommon.cginc" // ���� _LightColor0

				float _Outline;
				fixed4 _OutlineColor;
				float _HighLight;

				struct appdata
				{
					float4 pos : POSITION;
					float3 normal : NORMAL;
					float3 tangent : TANGENT; //�������߲�ֵ��
				};
				struct v2f
				{
					float4 pos : SV_POSITION;
					fixed4 diff : COLOR0; // ���������ɫ
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

					// ������ռ��л�ȡ���㷨��
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					// ��׼���䣨�����أ����յķ��ߺ�
					// ���߷���֮��ĵ��
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					// ����ǳɫ����
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
					// ��ӰͶ������Ⱦͨ����
		// ʹ�� UnityCG.cginc �еĺ��ֶ�ʵ��
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