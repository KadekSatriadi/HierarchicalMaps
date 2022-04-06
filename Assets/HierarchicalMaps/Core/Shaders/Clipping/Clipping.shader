Shader "IMMap/Clipping"
{
	Properties
	{
		_MainTex("Diffuse", 2D) = "white" {}
		[IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 0
	}
		SubShader
	{

		Tags {"Queue" = "Geometry"}

		ColorMask 0
		ZWrite off

		Stencil
		{
			Ref [_StencilRef]
			Comp always
			Pass replace
		}
		

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		uniform float _MapID;

	
		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}
		FallBack "Diffuse"
}
