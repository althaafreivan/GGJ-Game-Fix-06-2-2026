Shader "Custom/BillboardButtonOverlay"
{
    Properties
    {
        _MainTex ("Button Texture", 2D) = "white" {}
        [HDR] _Color ("Tint Color", Color) = (1, 1, 1, 1)
        _Scale ("Billboard Scale", Float) = 1.0
        _VerticalOffset ("Vertical Offset", Float) = 0.0
    }

    SubShader
    {
        // "Queue"="Overlay+100" to ensure it's rendered after most objects
        // "IgnoreProjector"="True" prevents light projections
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Overlay+100" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector"="True"
            "DisableBatching"="True" // Essential for Billboarding logic per object
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // Visual Settings
            Cull Off
            ZWrite Off
            ZTest Always // This makes it appear on top of 3D objects
            Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _MainTex;
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Scale;
                float _VerticalOffset;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                
                // Get the world position of the object center (pivot)
                float3 worldPos = TransformObjectToWorld(float3(0, 0, 0));
                
                // Add vertical offset in world space
                worldPos.y += _VerticalOffset;

                // Get the camera's Right and Up vectors from the View Matrix
                // This ensures the plane always faces the camera directly
                float3 worldRight = UNITY_MATRIX_V[0].xyz;
                float3 worldUp = UNITY_MATRIX_V[1].xyz;

                // Combine them with the vertex local position (v.positionOS)
                // Use the object's scale from the matrix to maintain inspector scaling control
                float3 billboardWorldPos = worldPos 
                    + worldRight * v.positionOS.x * _Scale
                    + worldUp * v.positionOS.y * _Scale;

                // Transform to Clip Space
                o.positionCS = TransformWorldToHClip(billboardWorldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);
                return texColor * _Color;
            }
            ENDHLSL
        }
    }
}
