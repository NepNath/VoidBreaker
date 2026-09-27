// VoidPortal.shader
// Version écrite à la main (URP, Unity 6) du matériau void.
// Doit être placé dans le même dossier que VoidPortal.hlsl.

Shader "Void/Void Portal"
{
    Properties
    {
        _Poly("Polygones", Range(3, 100)) = 35
        _Transparency("Transparence", Range(0, 1)) = 0.35
        _Stars("Etoiles", Range(0, 3)) = 0.25
        _Wobble("Mouvement", Range(0, 0.12)) = 0.04
        _WobbleSpeed("Vitesse", Range(0, 2)) = 0.4
        [Toggle] _HDRStars("Etoiles HDR (bloom)", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float _Poly;
            float _Transparency;
            float _Stars;
            float _Wobble;
            float _WobbleSpeed;
            float _HDRStars;
        CBUFFER_END
        ENDHLSL

        // -------------------------------------------------------------------
        // Rendu couleur
        // -------------------------------------------------------------------
        Pass
        {
            Name "VoidForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Void.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 viewDirWS = input.positionWS - GetCameraPositionWS();
                float3 color;
                VoidPortal_float(viewDirWS, _Poly, _Transparency, _Stars, _Wobble, _WobbleSpeed, _Time.y, _HDRStars, color);
                return float4(color, 1.0);
            }
            ENDHLSL
        }

        // -------------------------------------------------------------------
        // Profondeur (depth prepass, depth texture)
        // -------------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            Cull Back
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half DepthFrag(Varyings input) : SV_Target
            {
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
