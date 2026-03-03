Shader "SFS/Part" {
    Properties {
        _Intensity ("Shade Intensity", Float) = 0
        _BurnMarkTex ("Burn Stripes", 2D) = "white" {}
        _GradientTex ("Gradient", 2D) = "white" {}
        [PerRendererData] _ColorTexture ("Color Texture", 2D) = "white" {}
        [PerRendererData] _ShapeTexture ("Shape Texture", 2D) = "white" {}
        [PerRendererData] _ShadowTexture ("Shadow Texture", 2D) = "white" {}
        [PerRendererData] _BaseDepth ("Base Depth", Float) = 0.5
        [PerRendererData] _DepthMultiplier ("Depth Multiplier", Float) = 0.02
    }

    SubShader {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _ColorTexture;
            sampler2D _ShapeTexture;
            sampler2D _ShadowTexture;

            struct appdata {
                float4 vertex : POSITION;
                float3 uv     : TEXCOORD0;
                float3 uv2    : TEXCOORD1;
                float3 uv3    : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 uv  : TEXCOORD0;
                float3 uv2 : TEXCOORD1;
                float3 uv3 : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                o.uv2 = v.uv2;
                o.uv3 = v.uv3;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
    float2 colorUV  = i.uv.xy  / i.uv.z;
    float2 shapeUV  = i.uv2.xy / i.uv2.z;
    float2 shadowUV = i.uv3.xy / i.uv3.z;

    fixed4 colorTex  = tex2D(_ColorTexture,  colorUV);
    fixed4 shapeTex  = tex2D(_ShapeTexture,  shapeUV);
    fixed4 shadowTex = tex2D(_ShadowTexture, shadowUV);

    fixed4 col;
    // Remap shadow from [0,1] to [0.5,1] so darkest edge is 50% not 0%
    float3 shadow = shadowTex.rgb * 0.05 + 0.95;
col.rgb = colorTex.rgb * shadow;
    col.a   = shapeTex.a;

    clip(col.a - 0.01);

    return col;
}
            ENDCG
        }
    }

    FallBack "Unlit/Color"
}