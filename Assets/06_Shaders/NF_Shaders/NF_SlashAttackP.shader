Shader "NF/SlashAttackHDR"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _FlashColor ("Flash Color", Color) = (1, 0, 0, 1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _FlashColor;
            float _FlashAmount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Muestra el sprite
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Agrega brillo HDR (no clamps)
                half4 flash = _FlashColor * _FlashAmount;

                // 🔥 SUMA el color HDR para Bloom
                half4 finalColor = baseColor + flash;

                // Conserva alpha del sprite
                finalColor.a = baseColor.a;
                return finalColor;
            }
            ENDHLSL
        }
    }
}
