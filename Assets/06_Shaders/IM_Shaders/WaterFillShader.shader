Shader "Custom/WaterFillShader_Animated"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Wave Speed", Range(0,10)) = 2
        _Distortion ("Wave Distortion", Range(0,0.2)) = 0.05
        _Flow ("Flow Speed", Range(0,5)) = 1
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Speed;
            float _Distortion;
            float _Flow;
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
                // 🔹 Movimiento horizontal tipo flujo
                i.uv.x += _TimeY * _Flow * 0.1;

                // 🔹 Ondulación vertical visible
                float wave = sin((i.uv.y * 15.0) + (_TimeY * _Speed)) * _Distortion;
                i.uv.x += wave;

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
