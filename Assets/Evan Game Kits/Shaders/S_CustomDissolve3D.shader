Shader "Custom/Dissolve_3D"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth("Edge Width", Range(0, 0.2)) = 0.05
        [HDR] _EdgeColor("Edge Color", Color) = (1, 2, 0, 1) // High intensity for bloom
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off // Dissolve looks better when you see the inside of the mesh

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _DissolveAmount, _EdgeWidth;
                float4 _EdgeColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.uv).r;
                float4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                // Dissolve Math
                float threshold = 1.0 - _DissolveAmount;
                float dissolve = step(threshold, noise);
                float edge = step(threshold - _EdgeWidth, noise) - dissolve;

                col.rgb = lerp(col.rgb, _EdgeColor.rgb, edge);
                col.a *= (dissolve + edge);

                // Optimization: Discard pixels entirely for better performance
                if (col.a < 0.01) discard;

                return col;
            }
            ENDHLSL
        }
    }
}