Shader "Custom/dissolve3dportal"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        
        [KeywordEnum(LinearDissolve, ContinuousPortal)] _Mode("Effect Mode", Float) = 0
        
        [Header(Movement)]
        _ScanSpeed("Speed", Float) = 0.5
        _ScanWidth("Band Height", Range(0.01, 1.0)) = 0.2
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0.5
        
        [Header(Cylinder Volume)]
        _CylinderRadius("Cylinder Radius", Range(0.01, 2.0)) = 0.5
        _CylinderSoftness("Volume Softness", Range(0.01, 1.0)) = 0.5
        
        [Header(Rotation)]
        _RotationX("Rotate X", Range(-180, 180)) = 0
        _RotationZ("Rotate Z", Range(-180, 180)) = 0
        
        [Header(Visuals)]
        _NoiseStrength("Noise Distortion", Range(0, 1)) = 0.3
        [HDR] _EdgeColor("Edge Glow Color", Color) = (0, 1, 2, 1)
        _EdgeWidth("Edge Width", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Blend SrcAlpha One
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _MODE_LINEARDISSOLVE _MODE_CONTINUOUSPORTAL

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

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _ScanSpeed, _ScanWidth, _DissolveAmount;
                float _CylinderRadius, _CylinderSoftness;
                float _RotationX, _RotationZ;
                float _NoiseStrength, _EdgeWidth;
                float4 _EdgeColor;
            CBUFFER_END

            float3 RotatePosition(float3 pos, float angleX, float angleZ)
            {
                float radX = radians(angleX);
                float radZ = radians(angleZ);
                float s, c;
                sincos(radX, s, c);
                float3 rotX = float3(pos.x, pos.y * c - pos.z * s, pos.y * s + pos.z * c);
                sincos(radZ, s, c);
                return float3(rotX.x * c - rotX.y * s, rotX.x * s + rotX.y * c, rotX.z);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.uv + (_Time.x * 0.1)).r;
                float3 rotatedPos = RotatePosition(input.positionOS, _RotationX, _RotationZ);
                
                float alphaMask = 1.0;
                float edgeMask = 0.0;

                #if _MODE_LINEARDISSOLVE
                    // Mode 1: 3D Object-Space Dissolve
                    float currentHeight = rotatedPos.y + 0.5; 
                    currentHeight += (noise * _NoiseStrength);
                    float threshold = 1.0 - _DissolveAmount;
                    alphaMask = step(threshold, currentHeight);
                    edgeMask = step(threshold - _EdgeWidth, currentHeight) - alphaMask;
                #else
                    // Mode 2: Volumetric Cylinder Scanning
                    float radialDist = length(rotatedPos.xz);
                    float volume = smoothstep(_CylinderRadius, _CylinderRadius - _CylinderSoftness, radialDist);
                    
                    float verticalScan = frac(rotatedPos.y - (_Time.y * _ScanSpeed));
                    verticalScan += (noise * _NoiseStrength * 0.1);
                    verticalScan = frac(verticalScan);

                    alphaMask = smoothstep(0.0, _ScanWidth, verticalScan) * smoothstep(1.0, 1.0 - _ScanWidth, verticalScan);
                    alphaMask *= volume;
                    
                    edgeMask = saturate(smoothstep(_ScanWidth + _EdgeWidth, _ScanWidth, verticalScan) - alphaMask);
                    edgeMask *= volume;
                #endif

                float4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                col.a *= saturate(alphaMask + edgeMask);
                col.rgb += edgeMask * _EdgeColor.rgb * _EdgeColor.a;

                if (col.a < 0.001) discard;
                return col;
            }
            ENDHLSL
        }
    }
}
