Shader "Micelia/UI_ArrowGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowStrength ("Glow Strength", Range(0, 3)) = 1
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
        _GlowWidth ("Glow Width", Range(0.01, 1)) = 0.3
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
            float _GlowStrength;
            float _PulseSpeed;
            float _GlowWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Textura base
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Calcula distancia al centro del sprite
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // Pulso animado
                float pulse = abs(sin(_Time.y * _PulseSpeed));

                // Brillo desde el centro hacia afuera
                float glow = smoothstep(_GlowWidth, 0.0, dist) * pulse;

                // Combina la textura con el brillo blanco
                fixed4 glowColor = _GlowColor * glow * _GlowStrength;

                return texColor + glowColor;
            }
            ENDCG
        }
    }
}
