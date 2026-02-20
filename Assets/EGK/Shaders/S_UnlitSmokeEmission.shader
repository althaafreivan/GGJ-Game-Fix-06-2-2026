Shader "Custom/UnlitSmokeEmission"
{
    Properties
    {
        [Header(Base Texture and Roll)]
        _MainTex ("Smoke Texture (Alpha source)", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (1.0, 1.0, 1.0, 1)
        _RollSpeed ("UV Roll Speed", Float) = 1.5
        
        [Header(Vertex Animation)]
        [Toggle(VERTEX_DISPLACEMENT)] _UseVertexDisp("Enable Vertex Displacement", Float) = 1
        _DispSpeed ("Displacement Speed", Float) = 3.0
        _DispAmount ("Displacement Amount", Range(0, 0.5)) = 0.05
        _WaveFreq ("Wave Frequency", Float) = 8.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "UnlitEmissionPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature VERTEX_DISPLACEMENT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _EmissionColor;
            float _RollSpeed;
            float _DispSpeed;
            float _DispAmount;
            float _WaveFreq;

            Varyings vert (Attributes v)
            {
                Varyings o;

                float3 posInput = v.positionOS.xyz;
                #ifdef VERTEX_DISPLACEMENT
                    // Use Object Space Y to keep the wave consistent across all instances/passes
                    float wave = sin(_Time.y * _DispSpeed + v.positionOS.y * _WaveFreq);
                    posInput += v.normalOS * wave * _DispAmount;
                #endif
                
                o.positionCS = TransformObjectToHClip(posInput);

                // --- UV SCROLLING ---
                float2 scrolledUV = v.uv;
                scrolledUV.y -= _Time.y * _RollSpeed;
                o.uv = TRANSFORM_TEX(scrolledUV, _MainTex);
                
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Sample the texture alpha
                half4 texSample = tex2D(_MainTex, i.uv);

                // Final color is just the HDR Emission Color 
                // multiplied by any detail in the texture (if you want texture color)
                // or just the color itself.
                half3 finalColor = _EmissionColor.rgb * texSample.rgb;

                // Return final color with texture alpha
                return half4(finalColor, texSample.a * _EmissionColor.a);
            }
            ENDHLSL
        }
    }
}