// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//���Shader  
//by��puppet_master  
//2017.1.5  

Shader "Custom/MarkShader"
{
	Properties
	{
		_MarkTex1("Texture1", 2D) = "white" {}
		_MarkTex2("Texture2", 2D) = "white" {}
		_Blend("Blend", Range(0,2)) = 0
		_HighLight("HighLight", Range(0,1)) = 1
		_Factor("Factor",range(0,1)) = 0.5//������Զ
		_ToonEffect("Toon Effect",range(0,1)) = 0.5//��ͨ���̶ȣ�����Ԫ������Ԫ�Ľ����ߣ�
		_Steps("Steps of toon",range(0,9)) = 3//ɫ�ײ���
		_Color("Color", Color) = (0, 0, 0, 0)
	}
		SubShader
		{
			pass {
			Blend SrcAlpha OneMinusSrcAlpha
				Tags{"LightMode" = "ForwardBase"}
				Cull Back
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				float4 _LightColor0;
				float4 _Color;
				float _Steps;
				float _ToonEffect;
				sampler2D _MarkTex1;
				float4 _MarkTex1_ST;
				sampler2D _MarkTex2;
				float4 _MarkTex2_ST;
				float _Blend;
					 
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
				};

				v2f vert(appdata v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);//�л�����������
					o.normal = v.normal;
					o.lightDir = ObjSpaceLightDir(v.vertex);
					o.viewDir = ObjSpaceViewDir(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MarkTex1);
					return o;
				}
				float4 frag(v2f i) :COLOR
				{
					float4 c = 1;
					float3 N = normalize(i.normal);
					float3 viewDir = normalize(i.viewDir);
					float3 lightDir = normalize(i.lightDir);
					float diff = max(0,dot(N,i.lightDir));//�����������������ɫ
					diff = (diff + 1) / 2;//����������
					diff = smoothstep(0,1,diff);//ʹ��ɫƽ������[0,1]��Χ֮��
					float toon = floor(diff * _Steps) / _Steps;//����ɫ����ɢ����������diffuse��ɫ������_Steps�֣�_Steps����ɫ��������ɫ�������Ĵ���ʹɫ�׼���ƽ������ʾ
					diff = lerp(diff,toon,_ToonEffect);//�����ⲿ���ǿɿصĿ�ͨ���̶�ֵ_ToonEffect�����ڿ�ͨ����ʵ�ı���

					fixed4 col;
					if (_Blend < 1)
						col = fixed4(0, 0, 0, 0);
					else if(_Blend < 2)
						col = tex2D(_MarkTex1, i.uv);
					else 
						col = tex2D(_MarkTex2, i.uv);
					c = col * _LightColor0 * (diff)*_Color;//��������ɫ���
					return c;
				}
				ENDCG
			}//
			pass {//���ӵ��Դ��pass��Ⱦ
				Tags{"LightMode" = "ForwardAdd"}
				Blend One One
				Cull Back
				ZWrite Off
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				float4 _LightColor0;
				float4 _Color;
				float _Steps;
				float _ToonEffect;

				struct v2f {
					float4 pos:SV_POSITION;
					float3 lightDir:TEXCOORD0;
					float3 viewDir:TEXCOORD1;
					float3 normal:TEXCOORD2;
				};

				v2f vert(appdata_full v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.normal = v.normal;
					o.viewDir = ObjSpaceViewDir(v.vertex);
					o.lightDir = _WorldSpaceLightPos0 - v.vertex;

					return o;
				}
				float4 frag(v2f i) :COLOR
				{
					float4 c = 1;
					float3 N = normalize(i.normal);
					float3 viewDir = normalize(i.viewDir);
					float dist = length(i.lightDir);//��������Դ�ľ���
					float3 lightDir = normalize(i.lightDir);
					float diff = max(0,dot(N,i.lightDir));
					diff = (diff + 1) / 2;
					diff = smoothstep(0,1,diff);
					float atten = 1 / (dist);//���ݾ��Դ�ľ������˥��
					float toon = floor(diff * atten * _Steps) / _Steps;
					diff = lerp(diff,toon,_ToonEffect);

					half3 h = normalize(lightDir + viewDir);//����������
					float nh = max(0, dot(N, h));
					float spec = pow(nh, 32.0);//����߹�ǿ��
					float toonSpec = floor(spec * atten * 2) / 2;//�Ѹ߹�Ҳ��ɢ��
					spec = lerp(spec,toonSpec,_ToonEffect);//���ڿ�ͨ����ʵ�߹�ı���


					c = _Color * _LightColor0 * (diff + spec);//���������ɫ
					return c;
				}
				ENDCG
			}//
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