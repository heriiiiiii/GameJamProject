Shader "Custom/RootGlow"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _MaskMap ("Mask Texture", 2D) = "black" {}
        _BaseColor ("Base Color", Color) = (0.4, 1, 0.4, 1)
        _GlowColor ("Glow Color", Color) = (0.8, 1, 0.8, 1)
        _Speed ("Flow Speed", Range(-2, 2)) = 1
        _Intensity ("Glow Intensity", Range(0, 3)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BaseMap;
            sampler2D _MaskMap;
            float4 _BaseColor;
            float4 _GlowColor;
            float _Speed;
            float _Intensity;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // movimiento del brillo
                float2 uvFlow = i.uv;
                uvFlow.x += _Time.y * _Speed;

                // texturas
                fixed4 baseTex = tex2D(_BaseMap, i.uv) * _BaseColor;
                fixed4 maskTex = tex2D(_MaskMap, uvFlow);
                
                // mezcla
                float glow = saturate(maskTex.r * _Intensity);
                fixed4 glowColor = lerp(baseTex, _GlowColor, glow);

                glowColor.a = baseTex.a; // mantener transparencia original
                return glowColor;
            }
            ENDHLSL
        }
    }
}
