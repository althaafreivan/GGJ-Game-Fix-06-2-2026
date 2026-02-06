Shader "Custom/UI/UVScrollKawase"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR]_Color ("Tint", Color) = (1,1,1,1)
        
        _ScrollXSpeed("X Scroll Speed", Float) = 1.0
        _ScrollYSpeed("Y Scroll Speed", Float) = 0.0
        _BlurOffset("Kawase Offset", Range(0, 10)) = 1.0

        // Required for UI Masking
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }

        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0 // Higher target for better sampling performance

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // Automatically filled by Unity: x=1/w, y=1/h
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            float _ScrollXSpeed;
            float _ScrollYSpeed;
            float _BlurOffset;

            v2f vert(appdata_t v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float2 scroll = float2(_ScrollXSpeed, _ScrollYSpeed) * _Time.y;
                float2 uv = i.texcoord * _MainTex_ST.xy + _MainTex_ST.zw + scroll;

                // Kawase uses texel size to ensure blur is consistent across resolutions
                float2 res = _MainTex_TexelSize.xy * (_BlurOffset + 0.5);

                // --- 5-Tap Kawase Pattern ---
                // Sample 1: Center
                half4 col = tex2D(_MainTex, uv);
                
                // Samples 2-5: Corners
                col += tex2D(_MainTex, uv + float2(res.x, res.y));
                col += tex2D(_MainTex, uv + float2(-res.x, res.y));
                col += tex2D(_MainTex, uv + float2(res.x, -res.y));
                col += tex2D(_MainTex, uv + float2(-res.x, -res.y));

                half4 color = (col / 5.0 + _TextureSampleAdd) * i.color;

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                return color;
            }
            ENDHLSL
        }
    }
}