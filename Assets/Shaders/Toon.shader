// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Tut/Shader/Toon/toon" {
	Properties{
				_MainTex("Texture", 2D) = "white" {}
		_Color("Main Color",color) = (1,1,1,1)//�������ɫ
		_Outline("Thick of Outline",range(0,0.1)) = 0.02//������ߵĴ�ϸ
		_Factor("Factor",range(0,1)) = 0.5//������Զ
		_ToonEffect("Toon Effect",range(0,1)) = 0.5//��ͨ���̶ȣ�����Ԫ������Ԫ�Ľ����ߣ�
		_Steps("Steps of toon",range(0,9)) = 3//ɫ�ײ���
	}
		SubShader{
			pass {//�������ǰ��pass��Ⱦ
			Tags{"LightMode" = "Always"}
			Cull Front
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			float _Outline;
			float _Factor;


			struct v2f {
				float4 pos:SV_POSITION;

			};

			v2f vert(appdata_full v) {
				v2f o;
				float3 dir = normalize(v.tangent.xyz);
				float3 dir2 = v.normal;
				float D = dot(dir,dir2);
				dir = dir * sign(D);
				dir = dir * _Factor + dir2 * (1 - _Factor);
				v.vertex.xyz += dir * _Outline;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			float4 frag(v2f i) :COLOR
			{
				float4 c = 0;
				return c;
			}
			ENDCG
			}//end of pass
			pass {//ƽ�й�ĵ�pass��Ⱦ
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
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);//�л�����������
				o.normal = v.normal;
				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.viewDir = ObjSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
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
				float toon = floor(diff * _Steps) / _Steps;//����ɫ����ɢ��������diffuse��ɫ������_Steps�֣�_Steps����ɫ��������ɫ�������Ĵ���ʹɫ�׼���ƽ������ʾ
				diff = lerp(diff,toon,_ToonEffect);//�����ⲿ���ǿɿصĿ�ͨ���̶�ֵ_ToonEffect�����ڿ�ͨ����ʵ�ı���

				fixed4 col = tex2D(_MainTex, i.uv);
				c = col * _LightColor0 * (diff);//��������ɫ���
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
	}
}