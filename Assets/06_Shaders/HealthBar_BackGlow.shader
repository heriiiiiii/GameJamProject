Shader "Custom/HealthBar_BackGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (0.0, 1.0, 0.7, 1)
        _GlowStrength ("Glow Strength", Range(0, 3)) = 1
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            float _GlowStrength;
            float _PulseSpeed;
            float _TimeY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // ⚡ Detectar bordes (basado en transparencia)
                float edge = smoothstep(0.3, 0.7, col.a);

                // ✨ Pulso animado
                float pulse = (sin(_TimeY * _PulseSpeed) * 0.5 + 0.5) * _GlowStrength;

                // 💫 Combinar el color original con el brillo
                col.rgb += _GlowColor.rgb * pulse * (1 - edge);

                return col;
            }
            ENDCG
        }
    }
}
