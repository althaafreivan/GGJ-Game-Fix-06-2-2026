Shader "Custom/BillboardSpriteOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Scale ("Billboard Scale", Float) = 1.0
        _VerticalOffset ("Vertical Offset", Float) = 0.0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Overlay+100" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline" = "UniversalPipeline"
            "DisableBatching"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ PIXELSNAP_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                half4 color         : COLOR;
                float2 uv           : TEXCOORD0;
            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _Scale;
                float _VerticalOffset;
                half4 _RendererColor;
                float4 _Flip;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;

                // Get world position of the pivot
                float3 worldPos = TransformObjectToWorld(float3(0, 0, 0));
                worldPos.y += _VerticalOffset;

                // Billboard vectors from View Matrix
                float3 worldRight = UNITY_MATRIX_V[0].xyz;
                float3 worldUp = UNITY_MATRIX_V[1].xyz;

                // Sprite Flip logic
                float2 pos = v.positionOS.xy * _Flip.xy;

                // Construct billboard position
                float3 billboardWorldPos = worldPos 
                    + worldRight * pos.x * _Scale
                    + worldUp * pos.y * _Scale;

                o.positionCS = TransformWorldToHClip(billboardWorldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color * _RendererColor;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);
                half4 finalColor = texColor * i.color;
                
                // Premultiply alpha (standard for Unity Sprites)
                finalColor.rgb *= finalColor.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
