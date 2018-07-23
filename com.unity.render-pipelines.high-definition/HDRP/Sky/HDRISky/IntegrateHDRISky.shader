Shader "Hidden/HDRenderPipeline/IntegrateHDRI"
{
    Properties
    {
        [HideInInspector]
        _Cubemap ("", CUBE) = "white" {}
    }

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
#pragma enable_d3d11_debug_symbols

            #include "CoreRP/ShaderLibrary/Common.hlsl"
            #include "CoreRP/ShaderLibrary/Color.hlsl"
            #include "CoreRP/ShaderLibrary/ImageBasedLighting.hlsl"
            #include "../../ShaderVariables.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texCoord   : TEXCOORD0;
            };

            TextureCube<float4> _Cubemap;

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texCoord   = GetFullScreenTriangleTexCoord(input.vertexID);

                return output;
            }
            

            real IntegrateHDRISky(TEXTURECUBE_ARGS(skybox, sampler_skybox), real3 N, uint sampleCount = 250000)
            {
                /*
                float sum = 0.0;
                float dphi = 0.002;
                float dtheta = 0.002;
                for (float phi = 0; phi < 2.0 * PI; phi += dphi)
                {
                    for (float theta = 0; theta < PI / 2.0; theta += dtheta)
                    {
                        float3 L = TransformGLtoDX(SphericalToCartesian(phi, cos(theta)));
                        real val = Luminance(SAMPLE_TEXTURECUBE_LOD(skybox, sampler_skybox, L, 0).rgb);
                        sum += cos(theta) * sin(theta) * val;
                    }
                }

                sum *= dphi * dtheta;

                return sum;
                */
                
                real acc      = 0.0;

                // Add some jittering on Hammersley2d
                real2 randNum  = InitRandom(0.0);

                real3x3 localToWorld = GetLocalFrame(N);

                for (uint i = 0; i < sampleCount; ++i)
                {
                   // real2 u = Fibonacci2d(i, sampleCount);

                    real2 u = frac(randNum + Hammersley2d(i, sampleCount));

                    real NdotL;
                    real weightOverPdf;
                    real3 L;
                    
                    ImportanceSampleLambert(u, localToWorld, L, NdotL, weightOverPdf);

                    if (NdotL > 0.0)
                    {
                        real val = Luminance(SAMPLE_TEXTURECUBE_LOD(skybox, sampler_skybox, L, 0).rgb);
                        acc += PI * val;
                    }
                }

                return acc / sampleCount;
                */
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float3 N = float3(0.0, 1.0, 0.0);

                float intensity = IntegrateHDRISky(TEXTURECUBE_PARAM(_Cubemap, s_trilinear_clamp_sampler), N);

                return float4(intensity, 1.0, 1.0, 1.0);
            }

            ENDHLSL
        }
    }
    Fallback Off
}
