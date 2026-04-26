Shader "Shard/TestBreak6GPU"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutout"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            AlphaToMask Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Cutoff;
            CBUFFER_END

            UNITY_INSTANCING_BUFFER_START(PerInstance)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BreakMoveVectorWS)
                UNITY_DEFINE_INSTANCED_PROP(float, _BreakStartTime)
                UNITY_DEFINE_INSTANCED_PROP(float, _BreakInvDuration)
                UNITY_DEFINE_INSTANCED_PROP(float, _BreakActive)
            UNITY_INSTANCING_BUFFER_END(PerInstance)

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR0;
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float active = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _BreakActive);
                float startTime = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _BreakStartTime);
                float invDuration = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _BreakInvDuration);
                float progress = active > 0.5 ? saturate((_Time.y - startTime) * invDuration) : 0.0;
                float shrink = 1.0 - progress;

                float3 scaledPositionOS = input.positionOS.xyz * shrink;
                float3 positionWS = TransformObjectToWorld(scaledPositionOS);
                positionWS += UNITY_ACCESS_INSTANCED_PROP(PerInstance, _BreakMoveVectorWS).xyz * progress;

                Varyings output;
                output.positionHCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = _BaseColor;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * input.color;
                clip(color.a - _Cutoff);
                return color;
            }
            ENDHLSL
        }
    }
}
