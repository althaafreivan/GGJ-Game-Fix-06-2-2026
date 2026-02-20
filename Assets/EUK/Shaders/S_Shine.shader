Shader "Custom/UI/Shine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Shine Settings)]
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineWidth ("Shine Width", Range(0, 1)) = 0.1
        _ShineGlow ("Shine Glow", Range(0, 5)) = 1.0
        _ShineSpeed ("Shine Speed", Float) = 2.0
        _ShineInterval ("Interval (Time Between)", Float) = 3.0
        
        // Required for UI Masking
        _ShineIntensity("Shine Intensity", Range(0, 1)) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }

        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ShineColor;
            float _ShineWidth;
            float _ShineGlow;
            float _ShineSpeed;
            float _ShineInterval;
            float _ShineIntensity;
            float4 _ClipRect;

            v2f vert(appdata_t v) {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                half4 color = tex2D(_MainTex, i.texcoord) * i.color;
                
                // --- Shine Logic ---
                // Calculate time loop based on interval
                float totalCycle = _ShineInterval + 1.0; 
                float time = _Time.y * _ShineSpeed;
                float progress = fmod(time, totalCycle);

                // Create a diagonal line (x + y)
                // We offset it by -1 to 2 to ensure it starts/ends outside the image
                float shinePos = progress * 2.0 - 1.0;
                float edge = i.texcoord.x + i.texcoord.y; 
                
                // Generate the smooth band
                float shine = smoothstep(shinePos - _ShineWidth, shinePos, edge) - 
                             smoothstep(shinePos, shinePos + _ShineWidth, edge);
                
                // Apply the shine to the color (preserving alpha)
                color.rgb += shine * _ShineColor.rgb * _ShineGlow * _ShineIntensity * color.a;

                // UI Masking support
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                return color;
            }
            ENDHLSL
        }
    }
}