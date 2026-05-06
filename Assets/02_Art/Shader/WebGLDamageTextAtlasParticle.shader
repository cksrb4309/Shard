Shader "Hidden/Shard/WebGLDamageTextAtlasParticle"
{
    Properties
    {
        _MainTex ("Atlas", 2D) = "white" {}
        _OutlineTex ("Outline Atlas", 2D) = "black" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineStrength ("Outline Strength", Range(0,2)) = 1
        _CellSize ("Cell Size", Float) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _OutlineTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineStrength;
            float _CellSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv0 : TEXCOORD0;
                float4 custom1_t1 : TEXCOORD1;
                float4 custom1_t2 : TEXCOORD2;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;

                float2 baseUv = TRANSFORM_TEX(v.uv0.xy, _MainTex);

                float2 cellFromPackedUv = v.uv0.zw;
                float2 cellFromTexcoord1 = v.custom1_t1.xy;
                float2 cellFromTexcoord2 = v.custom1_t2.xy;
                float2 atlasCell = cellFromPackedUv;

                if (abs(atlasCell.x) + abs(atlasCell.y) < 0.001)
                {
                    atlasCell = cellFromTexcoord1;
                }
                if (abs(atlasCell.x) + abs(atlasCell.y) < 0.001)
                {
                    atlasCell = cellFromTexcoord2;
                }
                atlasCell = clamp(atlasCell, 0.0, 9.0);

                float2 atlasOffset = atlasCell * _CellSize;
                o.uv = atlasOffset + (baseUv * _CellSize);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 fillSample = tex2D(_MainTex, i.uv);
                fixed4 outlineSample = tex2D(_OutlineTex, i.uv);

                fixed fillAlpha = max(fillSample.a, max(fillSample.r, max(fillSample.g, fillSample.b)));
                fixed outlineAlpha = max(outlineSample.a, max(outlineSample.r, max(outlineSample.g, outlineSample.b)));
                fixed outlineOnlyAlpha = saturate(outlineAlpha - fillAlpha);

                fixed particleAlpha = i.color.a;
                fixed outlineFactor = outlineOnlyAlpha * _OutlineStrength * _OutlineColor.a;

                fixed3 fillColor = i.color.rgb * fillAlpha;
                fixed3 outlineColor = _OutlineColor.rgb * outlineFactor;

                fixed4 outColor;
                outColor.rgb = (fillColor + outlineColor) * particleAlpha;
                outColor.a = saturate((fillAlpha + outlineFactor) * particleAlpha);
                return outColor;
            }
            ENDHLSL
        }
    }
}
