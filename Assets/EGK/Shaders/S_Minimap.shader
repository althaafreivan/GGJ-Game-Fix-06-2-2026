Shader "Custom/Minimap/PostProcessor"
{
    Properties
    {
        [MainTexture] _MainTex ("Minimap Render Texture", 2D) = "white" {}
        _GridSize ("Grid Size", Float) = 10.0
        _GridOpacity ("Grid Opacity", Range(0, 1)) = 0.2
        _ScanlineSpeed ("Scanline Speed", Float) = 2.0
        _ScanlineDensity ("Scanline Density", Float) = 50.0
        _ColorTint ("Map Tint", Color) = (1, 1, 1, 1)
        _Hardness ("Circle Mask Hardness", Range(0.01, 0.5)) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float _GridSize, _GridOpacity, _ScanlineSpeed, _ScanlineDensity, _Hardness;
            float4 _ColorTint;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Base Map Color
                half4 mapCol = tex2D(_MainTex, input.uv) * _ColorTint;

                // 2. Simple Grid Effect
                float2 gridUV = frac(input.uv * _GridSize);
                float gridLine = step(0.95, gridUV.x) + step(0.95, gridUV.y);
                mapCol.rgb += gridLine * _GridOpacity;

                // 3. Moving Scanline Effect
                float scanline = sin(input.uv.y * _ScanlineDensity + _Time.y * _ScanlineSpeed) * 0.05;
                mapCol.rgb += scanline;

                // 4. Circular Alpha Masking
                float dist = distance(input.uv, float2(0.5, 0.5));
                half alpha = smoothstep(0.5, 0.5 - _Hardness, dist);

                return half4(mapCol.rgb, mapCol.a * alpha);
            }
            ENDHLSL
        }
    }
}