Shader "Custom/StylizedGrass"
{
    Properties
    {
        [Header(Colors)]
        _TopColor("Top Color", Color) = (0.5, 1, 0.5, 1)
        _BottomColor("Bottom Color", Color) = (0, 0.5, 0, 1)
        
        [Header(Toon Lighting)]
        _ShadowColor("Shadow Tint", Color) = (0.5, 0.5, 0.5, 1)
        _ToonThreshold("Toon Threshold", Range(-1, 1)) = 0.0
        _ToonSmoothness("Toon Smoothness", Range(0.001, 1.0)) = 0.1
        
        [Header(Wind)]
        _WindSpeed("Wind Speed", Float) = 1.0
        _WindStrength("Wind Strength", Float) = 0.1
        [Header(Ghibli Style)]
        _ColorRotation("Color Rotation", Range(0, 360)) = 0
        _ColorOffset("Color Offset", Range(-1, 1)) = 0
        _NormalUpBlend("Normal Up Blend (0 = Mesh Normals)", Range(0, 1)) = 0.0
        _WorldColorVariation("World Color Variation", Range(0, 1)) = 0.2
        _VariationScale("Variation Scale", Float) = 0.1
        
        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 0 // Default Off
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 300

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor, _BottomColor, _ShadowColor;
                float _ToonThreshold, _ToonSmoothness;
                float _WindSpeed, _WindStrength, _WindNoiseScale;
                float _NormalUpBlend, _WorldColorVariation, _VariationScale;
                float _ColorRotation, _ColorOffset;
            CBUFFER_END

            // Simple noise function for wind and variation
            float hash(float n) { return frac(sin(n) * 43758.5453123); }
            float noise(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float n = p.x + p.y * 57.0 + 113.0 * p.z;
                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                                 lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
                            lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                                 lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
            }

            float3 ApplyWind(float3 positionWS, float2 uv)
            {
                // Only sway the top (uv.y > 0)
                float windWeight = pow(uv.y, 1.5); 
                float time = _Time.y * _WindSpeed;
                
                // Use world position to create a wave effect
                float wave = sin(time + positionWS.x * 0.5 + positionWS.z * 0.5);
                float noiseVal = noise(positionWS * _WindNoiseScale + time) * 2.0 - 1.0;
                
                float3 windOffset = float3(wave + noiseVal, 0, wave * 0.5 + noiseVal);
                return positionWS + windOffset * _WindStrength * windWeight;
            }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                positionWS = ApplyWind(positionWS, v.uv);
                
                o.positionCS = TransformWorldToHClip(positionWS);
                o.positionWS = positionWS;
                
                // Blend with Mesh Normals instead of forcing Up
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.normalWS = normalize(lerp(normalWS, float3(0, 1, 0), _NormalUpBlend)); 
                
                o.uv = v.uv;
                o.shadowCoord = TransformWorldToShadowCoord(positionWS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                Light light = GetMainLight(i.shadowCoord);
                float3 normal = normalize(i.normalWS);
                
                float NdotL = dot(normal, light.direction);
                float lightIntensity = NdotL * light.shadowAttenuation;
                float ramp = smoothstep(_ToonThreshold - 0.005, _ToonThreshold + 0.005, lightIntensity);
                
                // Rotated Gradient
                float2 uv = i.uv - 0.5;
                float s, c;
                sincos(radians(_ColorRotation), s, c);
                float2 rotatedUV = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c) + 0.5;
                
                float3 baseColor = lerp(_BottomColor.rgb, _TopColor.rgb, saturate(rotatedUV.y + _ColorOffset));
                
                float varNoise = noise(i.positionWS * _VariationScale);
                baseColor = lerp(baseColor, baseColor * (0.85 + varNoise * 0.3), _WorldColorVariation);
                
                float3 litColor = baseColor * light.color;
                float3 shadowColor = baseColor * _ShadowColor.rgb;
                float3 finalColor = lerp(shadowColor, litColor, ramp);
                
                finalColor += SampleSH(float3(0, 1, 0)) * baseColor * 0.05;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                positionWS = ApplyWind(positionWS, v.uv);
                
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = _LightDirection;
                #else
                    float3 lightDirectionWS = _MainLightPosition.xyz;
                #endif

                o.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
