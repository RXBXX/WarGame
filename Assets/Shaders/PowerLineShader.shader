// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//���Shader  
//by��puppet_master  
//2017.1.5  

Shader "Custom/PowerLineShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0, 0, 0, 0)
	}
	SubShader
	{ 
			Tags
			{
				"Queue" = "Transparent+1"
				"RenderType" = "Transparent"
		}

		pass {
			Tags{"LightMode" = "ForwardBase"}
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha

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
			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 pos:SV_POSITION;
				float2 uv : TEXCOORD1;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			float4 frag(v2f i) :COLOR
			{
				float2 uv = i.uv + float2(_Time.y/2, 0);
				fixed4 col = tex2D(_MainTex, uv) * _Color;
				return col;
			}
			ENDCG
		}
	}
}