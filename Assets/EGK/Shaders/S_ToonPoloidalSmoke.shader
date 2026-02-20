Shader "Custom/ToonPoloidalSmoke"
{
    Properties
    {
        [Header(Base Texture and Roll)]
        _MainTex ("Smoke Texture (Alpha source)", 2D) = "white" {}
        _RollSpeed ("UV Roll Speed", Float) = 1.5
        
        [Header(Vertex Animation)]
        // Toggles vertex animation on/off
        [Toggle(VERTEX_DISPLACEMENT)] _UseVertexDisp("Enable Vertex Displacement", Float) = 1
        _DispSpeed ("Displacement Speed", Float) = 3.0
        _DispAmount ("Displacement Amount", Range(0, 0.5)) = 0.05
        _WaveFreq ("Wave Frequency", Float) = 8.0

        [Header(Toon Lighting)]
        // Colors for the lit and shaded parts of the smoke
        [HDR] _LightColor ("Lit Color", Color) = (1.0, 1.0, 1.0, 1)
        [HDR] _ShadowColor ("Shadow Color", Color) = (0.3, 0.3, 0.5, 1)
        // The point where light snaps to shadow (0 = dark side, 1 = light side)
        _ToonThreshold ("Toon Threshold", Range(-1, 1)) = 0.2
        // How soft the edge between light and shadow is
        _ToonSmoothness ("Edge Smoothness", Range(0.001, 0.2)) = 0.02
    }

    SubShader
    {
        // Transparent setup for smoke
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Back // Ensure backfaces get culled for correct lighting normals

        Pass
        {
            Name "ToonSmokePass"
            Tags { "LightMode" = "UniversalForward" } // Required for main directional light

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Required for lighting calculations in URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Compiler directive for the toggle feature
            #pragma shader_feature VERTEX_DISPLACEMENT

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
                float3 normalWS : TEXCOORD1; // World Space Normal is needed for lighting
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _RollSpeed;
            float _DispSpeed;
            float _DispAmount;
            float _WaveFreq;

            float4 _LightColor;
            float4 _ShadowColor;
            float _ToonThreshold;
            float _ToonSmoothness;

            Varyings vert (Attributes v)
            {
                Varyings o;

                // --- VERTEX DISPLACEMENT (Optional Roll) ---
                float3 posInput = v.positionOS.xyz;
                #ifdef VERTEX_DISPLACEMENT
                    // Create a wave based on time and the 'V' UV coordinate along the tube
                    float wave = sin(_Time.y * _DispSpeed + v.positionOS.y * _WaveFreq);
                    // Push vertex along its normal based on the wave
                    posInput += v.normalOS * wave * _DispAmount;
                #endif
                
                // Transform position to clip space
                o.positionCS = TransformObjectToHClip(posInput);

                // Transform normal to World Space for lighting
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                
                // --- UV SCROLLING (Visual Roll) ---
                float2 scrolledUV = v.uv;
                // Offset V coordinate over time
                scrolledUV.y -= _Time.y * _RollSpeed;
                o.uv = TRANSFORM_TEX(scrolledUV, _MainTex);
                
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // 1. Base Texture sample (mostly for Alpha pattern)
                half4 texSample = tex2D(_MainTex, i.uv);

                // 2. Lighting Calculation setup
                float3 normal = normalize(i.normalWS);
                // Get the main directional light in the scene
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);

                // 3. Calculate NdotL (Standard Lambert lighting term)
                // Dot product results in 1.0 facing light, 0.0 perpendicular, -1.0 facing away.
                float NdotL = dot(normal, lightDir);

                // 4. TOON RAMP CALCULATION
                // Instead of using NdotL directly, we use smoothstep to create a sharp transition
                // around the threshold value.
                // smoothstep creates a value between 0 and 1 based on where NdotL falls between the min and max edge.
                float toonRamp = smoothstep(_ToonThreshold - _ToonSmoothness, _ToonThreshold + _ToonSmoothness, NdotL);

                // 5. Pick Color based on Ramp
                // If ramp is 0, use shadow color. If 1, use light color.
                half3 finalToonColor = lerp(_ShadowColor.rgb, _LightColor.rgb, toonRamp);

                // Multiply by the actual scene light color/intensity
                finalToonColor *= mainLight.color;

                // 6. Combine final output
                // Use the RGB from the toon calculation, and Alpha from the texture sample
                return half4(finalToonColor, texSample.a);
            }
            ENDHLSL
        }
    }
}