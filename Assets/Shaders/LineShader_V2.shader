Shader "Custom/LineShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
        {
            //贴图带透明通道 ，半透明效果设置如下：
            tags{"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
            LOD 100
            Blend  SrcAlpha OneMinusSrcAlpha           //Blend选值为： SrcAlpha 和1-SrcAlpha  //也可测试为 DstColor SrcColor    //one one    

            Pass
            {
                Name "Simple"
                //Cull off //双面

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return fixed4(0, 1, 1, 1);
                //return col;
            }
            ENDCG
        }
        }
}