Shader "Custom/HealthBar_MetallicEdge"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (1, 1, 0.7, 1)
        _EdgeIntensity ("Edge Intensity", Range(0, 5)) = 2
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.2
        _Speed ("Scroll Speed", Range(-5, 5)) = 1
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
            float4 _EdgeColor;
            float _EdgeIntensity;
            float _EdgeWidth;
            float _Speed;
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

                // Detectar el borde basado en alpha
                float edge = smoothstep(_EdgeWidth, 1.0, col.a);

                // Efecto de brillo deslizante
                float glowMask = sin((i.uv.x + i.uv.y + _TimeY * _Speed) * 20.0) * 0.5 + 0.5;
                glowMask = pow(glowMask, 3.0); // suaviza la curva

                // Aplicar color y movimiento solo en bordes
                col.rgb += _EdgeColor.rgb * _EdgeIntensity * (1 - edge) * glowMask;

                return col;
            }
            ENDCG
        }
    }
}
