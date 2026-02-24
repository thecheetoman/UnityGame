Shader "SFS/Terrain" {
    Properties {
        _PlanetTexture ("Planet Texture", 2D) = "white" {}
        _TextureA ("Texture A", 2D) = "white" {}
        _TextureB ("Texture B", 2D) = "white" {}
        _TextureTerrain ("Texture Terrain", 2D) = "white" {}
        _RepeatA ("Repeat A", Vector) = (1,1,1,1)
        _RepeatB ("Repeat B", Vector) = (1,1,1,1)
        _RepeatTerrain ("Repeat Terrain", Vector) = (1,1,1,1)
        _SurfaceSize ("Texture Transition", Float) = 1
        _Min ("Min fade", Float) = 0
        _Max ("Max fade", Float) = 1
        _ShadowSize ("Transition shade", Float) = 1
        _ShadowIntensity ("Shade intensity", Float) = 1
        _Fog ("Atmosphere Color", Vector) = (1,1,1,1)
    }

    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _PlanetTexture;
            float4 _Fog;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv       = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_PlanetTexture, i.uv);

                // Subtle darkening toward the horizon using atmosphere color
                float dist = length(_WorldSpaceCameraPos.xz - i.worldPos.xz);
                float fogBlend = saturate(dist / 50000.0) * _Fog.a;
                col.rgb = lerp(col.rgb, _Fog.rgb, fogBlend);

                return col;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}