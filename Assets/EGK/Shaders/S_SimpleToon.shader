Shader "Custom/SimpleToon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _LightColor ("Lit Color", Color) = (1, 1, 1, 1)
        [HDR] _ShadowColor ("Shadow Color", Color) = (0.2, 0.2, 0.2, 1)
        [Toggle(_AMBIENT_ON)] _UseAmbient ("Use Skybox Ambient", Float) = 1
        
        [Header(Toon Settings)]
        _ToonThreshold ("Threshold", Range(-1, 1)) = 0.0
        _ToonSmoothness ("Smoothness", Range(0.001, 0.1)) = 0.01
        
        [Header(Specular)]
        [Toggle(_SPECULAR_ON)] _SpecularOn ("Enable Specular", Float) = 0
        [HDR] _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularSize ("Specular Size", Range(0, 1)) = 0.1
        _SpecularFalloff ("Specular Falloff", Range(0.001, 0.5)) = 0.05
        
        [Header(Rim Light)]
        [Toggle(_RIM_ON)] _RimOn ("Enable Rim Light", Float) = 0
        [HDR] _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3.0
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.5
        _RimSmoothness ("Rim Smoothness", Range(0.001, 0.5)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // URP Keywords for Shadows and Lights
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _SPECULAR_ON
            #pragma shader_feature_local _RIM_ON
            #pragma shader_feature_local _AMBIENT_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { 
                float4 positionOS : POSITION; 
                float3 normalOS : NORMAL; 
                float2 uv : TEXCOORD0; 
            };
            
            struct Varyings { 
                float4 positionCS : SV_POSITION; 
                float3 normalWS : TEXCOORD1; 
                float2 uv : TEXCOORD0; 
                float3 viewDirWS : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
            };

            sampler2D _MainTex;
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _LightColor, _ShadowColor;
                float _ToonThreshold, _ToonSmoothness;
                
                float4 _SpecularColor;
                float _SpecularSize;
                float _SpecularFalloff;
                
                float4 _RimColor;
                float _RimPower;
                float _RimThreshold;
                float _RimSmoothness;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.viewDirWS = GetWorldSpaceViewDir(o.positionWS);
                o.uv = v.uv; 
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 normal = normalize(i.normalWS);
                float3 viewDir = normalize(i.viewDirWS);
                
                // Shadow Calculation
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light light = GetMainLight(shadowCoord);
                float shadowAtten = light.shadowAttenuation;
                
                // Diffuse Toon Ramp
                float NdotL = dot(normal, light.direction);
                // Bias NdotL by shadow attenuation to integrate cast shadows into the toon ramp
                float litOrShadow = NdotL * shadowAtten; 
                float ramp = smoothstep(_ToonThreshold - _ToonSmoothness, _ToonThreshold + _ToonSmoothness, litOrShadow);
                
                float3 shadowColor = _ShadowColor.rgb;
                #if _AMBIENT_ON
                    float3 ambient = SampleSH(normal);
                    shadowColor = ambient * _ShadowColor.rgb; // Tint the ambient with shadow color
                #endif
                
                float3 litColor = _LightColor.rgb * light.color;

                half3 finalColor = lerp(shadowColor, litColor, ramp);
                
                // Specular
                #if _SPECULAR_ON
                    float3 halfVector = normalize(light.direction + viewDir);
                    float NdotH = dot(normal, halfVector);
                    float specIntensity = pow(saturate(NdotH), _SpecularSize * 100.0); // Simple Blinn-Phong base
                    float specRamp = smoothstep(1.0 - _SpecularSize, 1.0 - _SpecularSize + _SpecularFalloff, NdotH);
                    finalColor += _SpecularColor.rgb * specRamp * shadowAtten * light.color; // Specular needs light color
                #endif

                // Rim Light
                #if _RIM_ON
                    float NdotV = 1.0 - saturate(dot(normal, viewDir));
                    float rimIntensity = pow(NdotV, _RimPower);
                    float rimRamp = smoothstep(_RimThreshold - _RimSmoothness, _RimThreshold + _RimSmoothness, rimIntensity);
                    // Rim is usually additive, and often visible even in shadow, but arguably masked by direct light.
                    // Here we add it on top.
                    finalColor += _RimColor.rgb * rimRamp;
                #endif

                // Combine with Texture
                finalColor *= tex2D(_MainTex, i.uv).rgb;
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
        
        // ShadowCaster Pass - Essential for the object to cast shadows
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
