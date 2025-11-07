Shader "Micelia/Spawn_WhitePulse_AlphaOnly"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _GlowColor("Glow Color", Color) = (0.7, 1, 0.9, 1)
        _GlowIntensity("Glow Intensity", Range(0, 10)) = 4
        _PulseSpeed("Pulse Speed", Range(0, 10)) = 1
        _EdgeSoftness("Edge Softness", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off Lighting Off

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
            float4 _GlowColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _EdgeSoftness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Canal alfa como máscara de respiración (zonas opacas)
                float mask = smoothstep(_EdgeSoftness, 1.0, tex.a);

                // Pulso de respiración fuerte y notorio
                float pulse = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed);
                float glow = mask * _GlowIntensity * pulse;

                // Color base intacto, el blanco respira
                fixed3 color = tex.rgb + (_GlowColor.rgb * glow * mask);

                return fixed4(color, tex.a);
            }
            ENDCG
        }
    }
}
