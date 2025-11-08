Shader "Custom/TrailDash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TrailColor ("Trail Color", Color) = (0.2, 0.8, 1.0, 0.7)
        _FadeAmount ("Fade Amount", Range(0, 1)) = 0.3
        _Distortion ("Distortion", Range(0, 0.1)) = 0.01
        _TrailIntensity ("Trail Intensity", Range(1, 5)) = 2.5
        _Speed ("Speed", Float) = 10.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _TrailColor;
            float _FadeAmount;
            float _Distortion;
            float _TrailIntensity;
            float _Speed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                
                // Efecto de distorsión por velocidad
                float2 distortion = sin(OUT.texcoord * 20.0 + _Time.y * _Speed) * _Distortion;
                OUT.texcoord += distortion;
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Obtener el color original de la textura
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                
                // Si el pixel es transparente, lo descartamos
                if (texColor.a < 0.1)
                    discard;
                
                // Mezclar con el color del trail
                fixed4 finalColor = texColor;
                finalColor.rgb = lerp(finalColor.rgb, _TrailColor.rgb, _TrailColor.a);
                
                // Aplicar intensidad
                finalColor.rgb *= _TrailIntensity;
                
                // Aplicar fade basado en el alpha del vertex color
                finalColor.a *= IN.color.a * (1.0 - _FadeAmount);
                
                return finalColor;
            }
            ENDCG
        }
    }
}