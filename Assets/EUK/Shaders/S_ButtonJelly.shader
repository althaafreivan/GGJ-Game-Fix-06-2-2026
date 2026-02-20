Shader "Unlit/ButtonJelly"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WobbleSpeed ("Wobble Speed", Float) = 4.0
        _WobbleAmount ("Wobble Amount", Float) = 0.05
    }
    SubShader
    {
        // Change to Transparent for UI/Sprite usage
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha 
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WobbleSpeed;
            float _WobbleAmount;

            v2f vert (appdata v)
            {
                v2f o;
                
                // --- JELLY WOBBLE LOGIC ---
                // We use the vertex's own position (v.vertex.y and x) as an offset 
                // so different parts of the button wiggle at different times.
                float wobble = sin(_Time.y * _WobbleSpeed + (v.vertex.y * 5.0)) * _WobbleAmount;
                float wobbleX = cos(_Time.y * _WobbleSpeed + (v.vertex.x * 5.0)) * _WobbleAmount;
                
                v.vertex.x += wobble;
                v.vertex.y += wobbleX;
                // --------------------------

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}