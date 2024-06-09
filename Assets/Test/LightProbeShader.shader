//在这里里面使用 自定义的 cginc 来实现全局GI
//GI数据的准备
//烘培分支的判断
//GI的直接光实现
//GI的间接光实现
//再议ATTENUATION
//光照探针的支持
Shader "MyShader/P1_8_8"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            Tags{"LightMode" = "ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            #include "CGIncludes/MyGlobalIllumination.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                //定义第二套 UV ，appdata 对应的固定语义为 TEXCOORD1
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                float4 texcoord1 : TEXCOORD1;
                #endif
                half3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;

                float4 worldPos : TEXCOORD;
                //定义第二套UV
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                float4 lightmapUV : TEXCOORD1;
                #endif
                half3 worldNormal : NORMAL;

                half3 sh : TEXCOORD2;
                //1、使用 阴影采样 和 光照衰减的方案的 第一步
                //同时定义灯光衰减以及实时阴影采样所需的插值器
                UNITY_LIGHTING_COORDS(3,4)
                    //UNITY_SHADOW_COORDS(2)
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld,v.vertex);
                    o.worldNormal = UnityObjectToWorldNormal(v.normal);

                    //对第二套UV进行纹理采样
                    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                        o.lightmapUV.xy = v.texcoord1 * unity_LightmapST.xy + unity_LightmapST.zw;
                    #endif

                        //实现 球谐 或者 环境色 和 顶点照明 的计算
                        //SH/ambient and vertex lights
                        #ifndef LIGHTMAP_ON //当此对象没有开启静态烘焙时
                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                            o.sh = 0;
                            //近似模拟非重要级别的点光在逐顶点上的光照效果
                            #ifdef VERTEXLIGHT_ON
                                o.sh += Shade4PointLights(
                                unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
                                unity_LightColor[0].rgb,unity_LightColor[1].rgb,unity_LightColor[2].rgb,unity_LightColor[3].rgb,
                                unity_4LightAtten0,o.worldPos,o.worldNormal);
                            #endif
                            o.sh = ShadeSHPerVertex(o.worldNormal,o.sh);
                        #endif
                        #endif


                            //2、使用 阴影采样 和 光照衰减的方案的 第二步
                            UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy)
                                //TRANSFER_SHADOW(o)
                                return o;
                            }

                            fixed4 frag(v2f i) : SV_Target
                            {
                                //1、准备 SurfaceOutput 的数据
                                SurfaceOutput o;
                            //目前先初始化为0，使用Unity自带的方法，把结构体中的内容初始化为0
                            UNITY_INITIALIZE_OUTPUT(SurfaceOutput,o)
                            o.Albedo = 1;
                            o.Normal = i.worldNormal;

                            //1、代表灯光的衰减效果
                            //2、实时阴影的采样
                            UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos);


                            //2、准备 UnityGIInput 的数据
                            UnityGIInput giInput;
                            //初始化
                            UNITY_INITIALIZE_OUTPUT(UnityGIInput,giInput);
                            //修改用到的数据
                            giInput.light.color = _LightColor0;
                            giInput.light.dir = _WorldSpaceLightPos0;
                            giInput.worldPos = i.worldPos;
                            giInput.worldViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                            giInput.atten = atten;
                            giInput.ambient = 0;

                            #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                giInput.ambient = i.sh;
                            #else
                                giInput.ambient = 0.0;
                            #endif


                            #if defined(DYNAMICLIGHTMAP_ON) || defined(LIGHTMAP_ON)
                            giInput.lightmapUV = i.lightmapUV;
                            #endif

                            //3、准备 UnityGI 的数据
                            UnityGI gi;
                            //直接光照数据（主平行光）
                            gi.light.color = _LightColor0;
                            gi.light.dir = _WorldSpaceLightPos0;
                            //间接光照数据(目前先给0)
                            gi.indirect.diffuse = 0;
                            gi.indirect.specular = 0;

                            //GI的间接光照的计算 
                            LightingLambert_GI1(o,giInput,gi);
                            //查看Unity源码可知，计算间接光照最主要的函数就是
                            //inline UnityGI UnityGI_Base1(UnityGIInput data, half occlusion, half3 normalWorld)
                            //所以我们直接给 gi 赋值，可以不使用 LightingLambert_GI1
                            gi = UnityGI_Base1(giInput,1,o.Normal);

                            //GI的直接光照的计算
                            //我们在得到GI的数据后，对其进行Lambert光照模型计算，即可得到结果
                            fixed4 c = LightingLambert1(o,gi);

                            return c;
                            //return fixed4(gi.indirect.diffuse,1);
                            //return 1;
                        }
                        ENDCG
                    }

        //阴影的投射
        Pass
        {
                            //1、设置 "LightMode" = "ShadowCaster"
                            Tags{"LightMode" = "ShadowCaster"}
                            CGPROGRAM

                            #pragma vertex vert
                            #pragma fragment frag
                            //需要添加一个 Unity变体
                            #pragma multi_compile_shadowcaster

                            #include "UnityCG.cginc"

                            //声明消融使用的变量
                            float _Clip;
                            sampler2D _DissolveTex;
                            float4 _DissolveTex_ST;

                            //2、appdata中声明float4 vertex:POSITION;和half3 normal:NORMAL;这是生成阴影所需要的语义.
                            //注意：在appdata部分，我们几乎不要去修改名字 和 对应的类型。
                            //因为，在Unity中封装好的很多方法都是使用这些标准的名字
                            struct appdata
                            {
                                float4 vertex:POSITION;
                                half3 normal:NORMAL;
                                float4 uv:TEXCOORD;
                            };
                            //3、v2f中添加V2F_SHADOW_CASTER;用于声明需要传送到片断的数据.
                            struct v2f
                            {
                                float4 uv : TEXCOORD;
                                V2F_SHADOW_CASTER;
                            };
                            //4、在顶点着色器中添加TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)，主要是计算阴影的偏移以解决不正确的Shadow Acne和Peter Panning现象.
                            v2f vert(appdata v)
                            {
                                v2f o;
                                o.uv.zw = TRANSFORM_TEX(v.uv,_DissolveTex);
                                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                                return o;
                            }
                            //5、在片断着色器中添加SHADOW_CASTER_FRAGMENT(i)

                            fixed4 frag(v2f i) : SV_Target
                            {
                                //外部获取的 纹理 ，使用前都需要采样
                                fixed4 dissolveTex = tex2D(_DissolveTex,i.uv.zw);

                            //片段的取舍
                            clip(dissolveTex.r - _Clip);

                            SHADOW_CASTER_FRAGMENT(i);
                        }
                        ENDCG
                    }
    }
}

