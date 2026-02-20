Shader "Custom/SkyLantern"
{
    Properties
    {
        [Header(Colors)]
        [HDR] _TopColor("Top Color", Color) = (1, 0.5, 0.2, 1)
        [HDR] _BottomColor("Bottom Color", Color) = (1, 0.1, 0, 1)
        _Alpha("Base Alpha", Range(0, 1)) = 0.8
        
        [Header(Gradient Settings)]
        _Rotation("Gradient Rotation", Range(0, 360)) = 0
        _Scale("Gradient Scale", Float) = 1.0
        _Offset("Gradient Offset", Range(-1, 1)) = 0
        
        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 0 // Off
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull [_Cull]

        Pass
        {
            Name "Unlit"
            
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
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor, _BottomColor;
                float _Alpha;
                float _Rotation, _Scale, _Offset;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Gradient Rotation & Scaling
                float2 uv = i.uv - 0.5;
                float s, c;
                sincos(radians(_Rotation), s, c);
                float2 rotatedUV = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
                
                // Calculate gradient factor
                float gradient = saturate((rotatedUV.y * _Scale) + 0.5 + _Offset);
                
                // Mix colors
                float3 finalColor = lerp(_BottomColor.rgb, _TopColor.rgb, gradient);
                float finalAlpha = lerp(_BottomColor.a, _TopColor.a, gradient) * _Alpha;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}
