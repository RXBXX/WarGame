Shader "Unlit/BillboardShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_MainTex1("Texture1", 2D) = "white" {}
		[MaterialToggle]_Verical("Vercial",Range(0,1)) = 1
	}
		SubShader
	{
		Tags { "Quene" = "Transparent" "RenderType" = "Transparent"}
		Pass
		{
			Zwrite Off//不将此对象的像素写入深度缓冲区
			Blend SrcAlpha OneMinusSrcAlpha//混合要渲染像素的A通道和1-要渲染的像素的A通道
			//禁用剔除，绘制所有面
			Cull off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		#pragma multi_compile_instancing
		// make fog work
		//#pragma multi_compile_fog

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _MainTex1;
		float4 _MainTex1_ST;
		fixed _Verical;
		//float4 _Color;
		//float _TexIndex;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;

			UNITY_VERTEX_INPUT_INSTANCE_ID //启动GPUInstancing
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			//UNITY_FOG_COORDS(1)
			float4 vertex : SV_POSITION;
			float texIndex : TEXCOORD1;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float,_TexIndex)
		UNITY_INSTANCING_BUFFER_END(Props)

		v2f vert(appdata v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);

			// 在 fragment 中使用，需要在此进行设置
			UNITY_TRANSFER_INSTANCE_ID(v, o);

			float3 center = float3(0,0,0);
			//视角方向：摄像机的坐标减去物体的点
			float3 view = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1));
			float3 normalDir = view - center;
			//表面法线的变化：如果_Verical=1，则为表面法线，否则为向上方向
			normalDir.y = normalDir.y * _Verical;
			//归一化

			normalDir = normalize(normalDir);
			float3 upDir = abs(normalDir.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
			//叉乘  cross(A,B)返回两个三元向量的叉积(cross product)。注意，输入参数必须是三元向量
			float3 rightDir = normalize(cross(upDir,normalDir));
			upDir = normalize(cross(normalDir, rightDir));

			//计算中心点偏移
			float3 centerOffs = v.vertex.xyz - center;
			//位置的变换
			float3 localPos = center + rightDir * centerOffs.x + upDir * centerOffs.y + centerOffs.z;
			o.vertex = UnityObjectToClipPos(localPos);
			//o.vertex = UnityObjectToClipPos(v.vertex);
			o.texIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _TexIndex);

			if (o.texIndex < 1)
			    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			else
				o.uv = TRANSFORM_TEX(v.uv, _MainTex1);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;

			}
			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
			//return fixed4(i.texIndex, i.texIndex, i.texIndex, 1);
				// sample the texture
				fixed4 col;
				sampler2D tex;
				if (i.texIndex < 1)
					col = tex2D(_MainTex, i.uv);
				else
					col = tex2D(_MainTex1, i.uv);// *UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
