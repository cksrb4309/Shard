Shader "Shard/BreakBaseBlockGPU"
{
    Properties
    {
        [MainTexture] _MainTexture("Main Texture", 2D) = "white" {}
        _BreakParams("Break Params", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTexture);
            SAMPLER(sampler_MainTexture);

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BreakParams)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 shardCenterOS : TEXCOORD1;
                float4 shardMoveVectorOS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float4 breakParams = UNITY_ACCESS_INSTANCED_PROP(Props, _BreakParams);
                float breakStartTime = breakParams.x;
                float breakInvDuration = breakParams.y;
                float breakActive = breakParams.z;
                float progress = breakActive > 0.5 ? saturate((_Time.y - breakStartTime) * breakInvDuration) : 0.0;
                float shrink = 1.0 - progress;

                float3 centerOS = input.shardCenterOS.xyz;
                float3 moveVectorOS = input.shardMoveVectorOS.xyz;
                float3 localOffsetOS = input.positionOS.xyz - centerOS;
                float3 positionOS = centerOS + (localOffsetOS * shrink) + (moveVectorOS * progress);

                Varyings output;
                output.positionHCS = TransformObjectToHClip(positionOS);
                output.uv = input.uv;
                output.normalWS = normalize(TransformObjectToWorldNormal(input.normalOS));
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 sampleRgb = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, input.uv).rgb;
                half damageLerp = length(sampleRgb);

                half baseLighting = (abs(input.normalWS.x) + abs(input.normalWS.z)) * 0.15h;
                baseLighting += input.normalWS.y;

                half3 baseColor = baseLighting.xxx;
                half3 damageColor = sampleRgb * 0.28235295h;
                half3 finalColor = lerp(baseColor, damageColor, damageLerp);
                return half4(finalColor, 1.0h);
            }
            ENDHLSL
        }
    }
}
