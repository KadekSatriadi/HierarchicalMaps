//Kadek Satriadi
//Based on https://en.wikibooks.org/wiki/Cg_Programming/Unity/Cutaways
Shader "IMMap/ClipVolume" {
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	    _Height("_Height", Float) = 1
		_Width("_Width", Float) = 1
        _Length("_Length", Float) = 1
		_OffsetH("_OffsetH", Float) = 0
		_OffsetW("_OffsetW", Float) = 0
		_OffsetW("_OffsetL", Float) = 0
	}
	SubShader{		
		  Pass {

			Tags { "LightMode" = "ForwardBase" }
			 Cull Off // turn off triangle culling, alternatives are:
			 // Cull Back (or nothing): cull only back faces 
			 // Cull Front : cull only front faces

			 CGPROGRAM

			 #pragma vertex vert  
			 #pragma fragment frag 
			#include "UnityCG.cginc"

			 struct vertexOutput {
				float4 vertColors: COLOR;
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 posInObjectCoords : TEXCOORD1;
			 };

			 sampler2D _MainTex;
			 float4 _MainTex_ST;

			 float _Height;
			 float _Width;
			 float _Length;
			 float _OffsetH;
			 float _OffsetW;
			 float _OffsetL;

			 vertexOutput vert(appdata_full  v)
			 {
				vertexOutput o;

				o.vertColors = v.color.rgba;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.posInObjectCoords = v.vertex;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;
			 }

			 float4 frag(vertexOutput input) : SV_Target
			 {
				if (input.posInObjectCoords.y + _OffsetH >  _Height)
				{
				   discard; 
				}
				if (input.posInObjectCoords.y + _OffsetH < -_Height)
				{
					discard; 
				}
				if (input.posInObjectCoords.x + _OffsetW >  _Width)
				{
					discard; 
				}
				if (input.posInObjectCoords.x + _OffsetW < -_Width)
				{
					discard; 
				}
				if (input.posInObjectCoords.z + _OffsetL > _Length)
				{
					discard;
				}
				if (input.posInObjectCoords.z + _OffsetL < -_Length)
				{
					discard;
				}

				fixed4 c = tex2D(_MainTex, input.uv);
				return c;
			 }

			 ENDCG
		  }
	}
	Fallback "VertexLit"
}