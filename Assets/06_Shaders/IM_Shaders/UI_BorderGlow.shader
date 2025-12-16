Shader "Micelia/UI_BorderGlow"
{
    Properties
    {
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 1, 0.3, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1
        _Speed ("Scroll Speed", Range(0, 5)) = 1
        _Width ("Glow Width", Range(0.01, 1)) = 0.2
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

            struct appdata_t
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
            float _Speed;
            float _Width;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
{
    // Textura base
    fixed4 col = tex2D(_MainTex, i.uv);

    // Movimiento oscilante (de abajo hacia arriba y viceversa)
    float t = abs(frac(_Time.y * _Speed * 2.0) * 2.0 - 1.0);

    // Calcula máscara de brillo
    float glowMask = smoothstep(t - _Width, t, i.uv.y) * smoothstep(t + _Width, t, i.uv.y);

    // Combinar con color del brillo
    fixed4 glow = _GlowColor * glowMask * _GlowIntensity;

    return fixed4(col.rgb + glow.rgb, col.a);
}

            ENDCG
        }
    }
}
