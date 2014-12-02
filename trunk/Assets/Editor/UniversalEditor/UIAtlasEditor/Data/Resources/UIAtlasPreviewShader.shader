Shader "Hidden/UIAtlasPreviewShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert finalcolor:finalColorProc

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		void finalColorProc(Input IN, SurfaceOutput s , inout fixed4 color)
		{
			color = tex2D (_MainTex, IN.uv_MainTex); 
		}

		ENDCG
	} 
	FallBack "Diffuse"
}
