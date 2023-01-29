// 

Shader "tpr/ground"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1) // 颜色来源之一
    }

    SubShader
    {
        Tags{
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {

            Name "tpr_ground"

            HLSLPROGRAM

            #pragma vertex vert 
            #pragma fragment frag 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 为支持 SRP Batcher 功能, 所有 material properties 都要被整合进 cbuffer 中
            // 参数是 cbuffer 的名字
            CBUFFER_START(UnityPerMaterial)
                half4  _BaseColor;


            CBUFFER_END


            float4 _gNodeDatas[100]; // xpz:pos 


            struct Attributes
            {
                float4 posOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
            };


            Varyings vert( Attributes i )
            {

                Varyings o = (Varyings)0;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(i.posOS.xyz);
                o.positionHCS = vertexInput.positionCS;
                o.positionWS.xyz = vertexInput.positionWS;
                return o;
            }


            half4 frag( Varyings input ) : SV_Target
            {

                float fstMinDistance2 = 100.0;
                float secMinDistance2 = 100.0;
                int tgtIdx = 0;


                // 成本很高...
                // 
                for( uint i = 0; i<100; i++ ) 
                {
                    float3 offset = float3(
                        input.positionWS.x - _gNodeDatas[i].x,
                        0.0,
                        input.positionWS.z - _gNodeDatas[i].z
                    );

                    
                    float dis2 = dot(offset,offset);
                    if( dis2 < fstMinDistance2 )
                    {
                        tgtIdx = i;
                        secMinDistance2 = fstMinDistance2;
                        fstMinDistance2 = dis2;
                    }
                }

                float t = abs( secMinDistance2 - fstMinDistance2 ) / 5.0;

                //float t = fstMinDistance2 / 100.0;
                t = t * t * t;

                half4 color = half4( _gNodeDatas[tgtIdx].w, t, t, 1 );
                //half4 color = half4( _BaseColor.xyz, 1 );
                return color;
            }

            ENDHLSL
        }
    }


}
