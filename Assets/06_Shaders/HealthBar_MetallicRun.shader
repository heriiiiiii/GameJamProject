Shader "Custom/HealthBar_ShineBreath_Dynamic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShineColor ("Shine Color", Color) = (1, 0.95, 0.6, 1)
        _BaseBoost ("Base Brightness", Range(0,2)) = 1.0
        _ShineIntensity ("Shine Intensity", Range(0,10)) = 2.0
        _Speed ("Shine Scroll Speed", Range(-10,10)) = 3.0
        _PulseSpeed ("Width Pulse Speed", Range(0,10)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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
            float4 _ShineColor;
            float _BaseBoost;
            float _ShineIntensity;
            float _Speed;
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
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                baseCol.rgb *= _BaseBoost;

                // 🔹 Oscilación automática del ShineWidth entre 0.0 y 0.5
                float dynamicWidth = (sin(_TimeY * _PulseSpeed) * 0.5 + 0.5) * 0.5;

                // 🔹 Movimiento del brillo
                float2 movingUV = i.uv;
                movingUV.x += _TimeY * _Speed * 0.1;
                movingUV.y += _TimeY * _Speed * 0.05;

                // 🔹 Generar máscara de brillo según el ancho dinámico
                float shineMask = abs(frac(movingUV.x + movingUV.y) - 0.5);
                shineMask = smoothstep(0.5 - dynamicWidth, 0.5, shineMask);

                // 🔹 Combinar color con intensidad
                baseCol.rgb += _ShineColor.rgb * shineMask * _ShineIntensity;

                return baseCol;
            }
            ENDCG
        }
    }
}
