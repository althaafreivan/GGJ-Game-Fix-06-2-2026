Shader "Custom/PosterEmission"
{
    Properties
    {
        [Header(Base)]
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        
        [Header(Emission)]
        [HDR] _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 1.0
        
        [Header(Loop Effect)]
        _EffectSpeed ("Effect Speed", Float) = 0.5
        _EffectWidth ("Effect Width", Range(0.01, 1)) = 0.1
        _EffectHardness ("Effect Hardness", Range(0, 10)) = 2.0
        [HDR] _EffectColor ("Effect Color", Color) = (2, 2, 2, 1)
        _EffectDirection ("Effect Direction (X or Y)", Vector) = (0, 1, 0, 0)

        [Header(Alpha)]
        _AlphaMultiplier ("Alpha Multiplier", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "Unlit"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
                half4 _EmissionColor;
                half _EmissionIntensity;
                half _EffectSpeed;
                half _EffectWidth;
                half _EffectHardness;
                half4 _EffectColor;
                float4 _EffectDirection;
                half _AlphaMultiplier;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 baseRGB = texColor.rgb * _BaseColor.rgb;
                
                // --- Loop Effect (Scrolling Glow Wave) ---
                // Calculate moving coordinate based on direction
                float moveCoord = dot(input.uv, _EffectDirection.xy);
                float time = _Time.y * _EffectSpeed;
                
                // Create a repeating sawtooth-like wave [0, 1]
                float wavePos = frac(time);
                
                // Calculate distance from current wave position
                // We use frac to make it wrap around smoothly for the distance check
                float dist = abs(moveCoord - wavePos);
                // Handle wrapping for distance
                dist = min(dist, abs(moveCoord - (wavePos - 1.0)));
                dist = min(dist, abs(moveCoord - (wavePos + 1.0)));

                // Shape the wave
                float glow = saturate(1.0 - (dist / _EffectWidth));
                glow = pow(glow, _EffectHardness);
                
                half3 effectEmission = _EffectColor.rgb * glow;
                half3 constantEmission = _EmissionColor.rgb * _EmissionIntensity;
                
                half3 finalRGB = baseRGB + constantEmission + effectEmission;
                
                // Use texture alpha
                half finalAlpha = texColor.a * _BaseColor.a * _AlphaMultiplier;

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}
