Shader "Custom/Effects/WiggleOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _WiggleSpeed ("Wiggle Speed", Float) = 2.0
        _WiggleAmount ("Wiggle Amount", Float) = 0.05
        _WiggleFrequency ("Wiggle Frequency", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _WiggleSpeed;
            float _WiggleAmount;
            float _WiggleFrequency;

            v2f vert (appdata v)
            {
                v2f o;

                // Calculate a wiggle factor based on world position and time
                // This makes different parts of the mesh wiggle at different rates
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Displacement Formula:
                // displacement = sin(pos.y * freq + time * speed) * amount
                float displacement = sin(worldPos.y * _WiggleFrequency + _Time.y * _WiggleSpeed) 
                                   * cos(worldPos.x * _WiggleFrequency * 0.5 + _Time.y) 
                                   * _WiggleAmount;

                // Push the vertex along its normal
                v.vertex.xyz += v.normal * displacement;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Return the outline color
                return _OutlineColor;
            }
            ENDCG
        }
    }
}