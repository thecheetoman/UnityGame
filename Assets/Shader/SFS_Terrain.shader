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
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
}