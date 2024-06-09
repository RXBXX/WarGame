//����������ʹ�� �Զ���� cginc ��ʵ��ȫ��GI
//GI���ݵ�׼��
//�����֧���ж�
//GI��ֱ�ӹ�ʵ��
//GI�ļ�ӹ�ʵ��
//����ATTENUATION
//����̽���֧��
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
                //����ڶ��� UV ��appdata ��Ӧ�Ĺ̶�����Ϊ TEXCOORD1
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                float4 texcoord1 : TEXCOORD1;
                #endif
                half3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;

                float4 worldPos : TEXCOORD;
                //����ڶ���UV
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                float4 lightmapUV : TEXCOORD1;
                #endif
                half3 worldNormal : NORMAL;

                half3 sh : TEXCOORD2;
                //1��ʹ�� ��Ӱ���� �� ����˥���ķ����� ��һ��
                //ͬʱ����ƹ�˥���Լ�ʵʱ��Ӱ��������Ĳ�ֵ��
                UNITY_LIGHTING_COORDS(3,4)
                    //UNITY_SHADOW_COORDS(2)
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld,v.vertex);
                    o.worldNormal = UnityObjectToWorldNormal(v.normal);

                    //�Եڶ���UV�����������
                    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                        o.lightmapUV.xy = v.texcoord1 * unity_LightmapST.xy + unity_LightmapST.zw;
                    #endif

                        //ʵ�� ��г ���� ����ɫ �� �������� �ļ���
                        //SH/ambient and vertex lights
                        #ifndef LIGHTMAP_ON //���˶���û�п�����̬�決ʱ
                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                            o.sh = 0;
                            //����ģ�����Ҫ����ĵ�����𶥵��ϵĹ���Ч��
                            #ifdef VERTEXLIGHT_ON
                                o.sh += Shade4PointLights(
                                unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
                                unity_LightColor[0].rgb,unity_LightColor[1].rgb,unity_LightColor[2].rgb,unity_LightColor[3].rgb,
                                unity_4LightAtten0,o.worldPos,o.worldNormal);
                            #endif
                            o.sh = ShadeSHPerVertex(o.worldNormal,o.sh);
                        #endif
                        #endif


                            //2��ʹ�� ��Ӱ���� �� ����˥���ķ����� �ڶ���
                            UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy)
                                //TRANSFER_SHADOW(o)
                                return o;
                            }

                            fixed4 frag(v2f i) : SV_Target
                            {
                                //1��׼�� SurfaceOutput ������
                                SurfaceOutput o;
                            //Ŀǰ�ȳ�ʼ��Ϊ0��ʹ��Unity�Դ��ķ������ѽṹ���е����ݳ�ʼ��Ϊ0
                            UNITY_INITIALIZE_OUTPUT(SurfaceOutput,o)
                            o.Albedo = 1;
                            o.Normal = i.worldNormal;

                            //1������ƹ��˥��Ч��
                            //2��ʵʱ��Ӱ�Ĳ���
                            UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos);


                            //2��׼�� UnityGIInput ������
                            UnityGIInput giInput;
                            //��ʼ��
                            UNITY_INITIALIZE_OUTPUT(UnityGIInput,giInput);
                            //�޸��õ�������
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

                            //3��׼�� UnityGI ������
                            UnityGI gi;
                            //ֱ�ӹ������ݣ���ƽ�й⣩
                            gi.light.color = _LightColor0;
                            gi.light.dir = _WorldSpaceLightPos0;
                            //��ӹ�������(Ŀǰ�ȸ�0)
                            gi.indirect.diffuse = 0;
                            gi.indirect.specular = 0;

                            //GI�ļ�ӹ��յļ��� 
                            LightingLambert_GI1(o,giInput,gi);
                            //�鿴UnityԴ���֪�������ӹ�������Ҫ�ĺ�������
                            //inline UnityGI UnityGI_Base1(UnityGIInput data, half occlusion, half3 normalWorld)
                            //��������ֱ�Ӹ� gi ��ֵ�����Բ�ʹ�� LightingLambert_GI1
                            gi = UnityGI_Base1(giInput,1,o.Normal);

                            //GI��ֱ�ӹ��յļ���
                            //�����ڵõ�GI�����ݺ󣬶������Lambert����ģ�ͼ��㣬���ɵõ����
                            fixed4 c = LightingLambert1(o,gi);

                            return c;
                            //return fixed4(gi.indirect.diffuse,1);
                            //return 1;
                        }
                        ENDCG
                    }

        //��Ӱ��Ͷ��
        Pass
        {
                            //1������ "LightMode" = "ShadowCaster"
                            Tags{"LightMode" = "ShadowCaster"}
                            CGPROGRAM

                            #pragma vertex vert
                            #pragma fragment frag
                            //��Ҫ���һ�� Unity����
                            #pragma multi_compile_shadowcaster

                            #include "UnityCG.cginc"

                            //��������ʹ�õı���
                            float _Clip;
                            sampler2D _DissolveTex;
                            float4 _DissolveTex_ST;

                            //2��appdata������float4 vertex:POSITION;��half3 normal:NORMAL;����������Ӱ����Ҫ������.
                            //ע�⣺��appdata���֣����Ǽ�����Ҫȥ�޸����� �� ��Ӧ�����͡�
                            //��Ϊ����Unity�з�װ�õĺܶ෽������ʹ����Щ��׼������
                            struct appdata
                            {
                                float4 vertex:POSITION;
                                half3 normal:NORMAL;
                                float4 uv:TEXCOORD;
                            };
                            //3��v2f�����V2F_SHADOW_CASTER;����������Ҫ���͵�Ƭ�ϵ�����.
                            struct v2f
                            {
                                float4 uv : TEXCOORD;
                                V2F_SHADOW_CASTER;
                            };
                            //4���ڶ�����ɫ�������TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)����Ҫ�Ǽ�����Ӱ��ƫ���Խ������ȷ��Shadow Acne��Peter Panning����.
                            v2f vert(appdata v)
                            {
                                v2f o;
                                o.uv.zw = TRANSFORM_TEX(v.uv,_DissolveTex);
                                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                                return o;
                            }
                            //5����Ƭ����ɫ�������SHADOW_CASTER_FRAGMENT(i)

                            fixed4 frag(v2f i) : SV_Target
                            {
                                //�ⲿ��ȡ�� ���� ��ʹ��ǰ����Ҫ����
                                fixed4 dissolveTex = tex2D(_DissolveTex,i.uv.zw);

                            //Ƭ�ε�ȡ��
                            clip(dissolveTex.r - _Clip);

                            SHADOW_CASTER_FRAGMENT(i);
                        }
                        ENDCG
                    }
    }
}

