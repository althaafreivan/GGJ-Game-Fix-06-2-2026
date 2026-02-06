Shader "VFX/StarOverlay"
{
    Properties
    {
        _MainTex ("Star Texture", 2D) = "white" {}
        [HDR] _Color ("Emission Color", Color) = (1,1,1,1) // [HDR] allows high intensity for Bloom
    }
    SubShader
    {
        // "Overlay" queue ensures it draws last
        // "IgnoreProjector" prevents other lights/projectors from affecting it
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        
        // VISUAL SETTINGS
        Cull Off        // Render both sides of the face
        Lighting Off    // Unlit
        ZWrite Off      // Don't write to depth buffer (standard for VFX)
        
        // THE "ON TOP" MAGIC
        // "Always" makes the GPU render this pixel regardless of depth/walls
        ZTest Always    
        
        // BLENDING
        // SrcAlpha One = Additive Blending (Best for glowing lights/stars)
        // SrcAlpha OneMinusSrcAlpha = Normal Transparency
        Blend SrcAlpha One 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR; // Support for Particle System vertex colors if needed
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color; // Pass vertex color through
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Multiply texture by the HDR Color property and Vertex Color
                return col * _Color * i.color;
            }
            ENDCG
        }
    }
}