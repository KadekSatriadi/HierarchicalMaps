// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//Kadek Satriadi
//Based on https://en.wikibooks.org/wiki/Cg_Programming/Unity/Cutaways
Shader "IMMap/ClipVolumeGlobal" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Height("Height", Float) = 1
		_Width("Width", Float) = 1
		_Length("Length", Float) = 1
		_P1("P1", Float) = (0,0,0,0)
		_P1("P2", Float) = (0,0,0,0)
		_P1("P4", Float) = (0,0,0,0)
		_P1("P5", Float) = (0,0,0,0)
	}
		SubShader{
			  Tags {"Queue" = "Geometry" }

			  Pass {

				Tags { "LightMode" = "ForwardBase" }

				 CGPROGRAM

				 #pragma vertex vert  
				 #pragma fragment frag 

				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc" // for _LightColor0

				 struct vertexOutput {
					float4 vertColors: COLOR;
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 posInObjectCoords : TEXCOORD1;
					float3 worldPos: TEXCOORD2;
					fixed4 diff : COLOR1;
				 };

				 sampler2D _MainTex;
				 float4 _MainTex_ST;
				 float _Height;
				 float _Width;
				 float _Length;
				 float3 _P1, _P2, _P4, _P5;
				 
				 vertexOutput vert(appdata_full  v)
				 {
					vertexOutput o;

					o.vertColors = v.color.rgba;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.posInObjectCoords = v.vertex;
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					float3 worldPos  = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.worldPos = worldPos;
					// get vertex normal in world space
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					// dot product between normal and light direction for
					// standard diffuse (Lambert) lighting
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					// factor in the light color
					o.diff = nl * _LightColor0;

					return o;
				 }

				 float4 frag(vertexOutput input) : SV_Target
				 {
					float3 i = _P2 - _P1;
					float3 j = _P4 - _P1;
					float3 k = _P5 - _P1;
					float3 v = input.worldPos - _P1;

					//source: Trey Reynolds, https://math.stackexchange.com/questions/1472049/check-if-a-point-is-inside-a-rectangular-shaped-area-3d			
					bool test =
						0 < dot(v, i) && dot(v, i) < dot(i, i) &&
						0 < dot(v, j) && dot(v, j) < dot(j, j) &&
						0 < dot(v, k) && dot(v, k) < dot(k, k);

				if(!test) discard;
					

					// sample texture
					fixed4 col = tex2D(_MainTex, input.uv);
					// multiply by lighting
					//col *= input.diff;
					return col;
				 }

				 ENDCG
			  }
		}
}