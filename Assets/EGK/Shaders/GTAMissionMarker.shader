Shader "Custom/GTAMissionMarker"
{
    Properties
    {
        [HDR] _Color("Marker Color", Color) = (1, 0.9, 0, 1)
        _Radius("Radius", Range(0.1, 5.0)) = 1.0
        _Height("Height", Range(0.1, 5.0)) = 2.0
        _Softness("Edge Softness", Range(0.01, 1.0)) = 0.5
        
        [Header(Animation)]
        _ScrollSpeed("Pulse Speed", Float) = 1.0
        _PulseFrequency("Pulse Frequency", Float) = 2.0
        _PulseWidth("Pulse Width", Range(0.01, 1.0)) = 0.3
        
        [Header(Detail)]
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseStrength("Noise Distortion", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector"="True"
        }
        
        // Additive blending for that classic "glow" look
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes 
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings 
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
            };

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Radius, _Height, _Softness;
                float _ScrollSpeed, _PulseFrequency, _PulseWidth;
                float _NoiseStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Scale the cylinder mesh based on properties
                float3 scaledPos = input.positionOS.xyz;
                scaledPos.xz *= _Radius;
                scaledPos.y *= _Height;

                output.positionCS = TransformObjectToHClip(scaledPos);
                output.positionOS = scaledPos;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Calculate Cylinder Volume (XZ Radius)
                float radialDist = length(input.positionOS.xz);
                float volumeMask = smoothstep(_Radius, _Radius - _Softness, radialDist);
                
                // 2. Vertical Fade (Cylinder top/bottom fade)
                float verticalFade = smoothstep(0, _Softness, input.positionOS.y) * 
                                   smoothstep(_Height, _Height - _Softness, input.positionOS.y);
                
                // 3. Noise Distortion
                float2 noiseUV = input.uv + float2(0, _Time.y * 0.1);
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                // 4. Looping Pulse Effect (The "GTA" vertical scanning)
                // We use frac for a continuous zero-delay loop
                float scroll = frac((input.positionOS.y * _PulseFrequency) - (_Time.y * _ScrollSpeed));
                scroll += (noise * _NoiseStrength);
                scroll = frac(scroll); // Re-frac after noise

                // Create a sharp-ish band that travels up
                float pulse = smoothstep(1.0 - _PulseWidth, 1.0, scroll);
                
                // 5. Combine and Color
                // Base glow + the moving pulse
                float finalAlpha = (pulse + 0.2) * volumeMask * verticalFade;
                
                float4 finalColor = _Color * finalAlpha;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
