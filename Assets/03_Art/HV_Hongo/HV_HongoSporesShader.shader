Shader "Custom/HongoSpores_ScreenSpace"
{
    Properties
    {
        [PerRendererData] _BaseMap("Sprite Texture", 2D) = "white" {}
        _BaseColor("Tint", Color) = (1,1,1,1)
        _SporeColor("Spore Color", Color) = (0.31, 1, 0.92, 1)
        _SporeDensity("Spore Density", Range(0.3, 3)) = 1.6
        _SporeSpeed("Spore Rise Speed", Range(0.0, 3)) = 0.65
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Sprite"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseColor;
            float4 _SporeColor;
            float _SporeDensity;
            float _SporeSpeed;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Color base real del sprite
                float4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;

                // Convertimos world position → screen position
                float4 sp = ComputeScreenPos(i.pos);
                float2 sUV = sp.xy / sp.w;

                // Generar esporas en coordenadas de pantalla (no atlas)
                float2 grid = float2(40.0, 25.0) * _SporeDensity;
                float2 cell = floor(sUV * grid);
                float2 uvCell = frac(sUV * grid);

                float n = hash(cell);
                float t = frac(_Time.y * _SporeSpeed + n);

                float2 sporePos = float2(n, frac(n * 1.732));
                sporePos.y = frac(sporePos.y + t * 0.6);

                float dist = distance(uvCell, sporePos);
                float s = smoothstep(0.08, 0.018, dist);
                s *= (1.0 - t);

                float4 spores = _SporeColor * s;
                spores.rgb *= spores.a;

                return col + spores;
            }
            ENDHLSL
        }
    }
}
