Shader "IMMap/ClippedColor"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		[IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 0

    }
		SubShader
		{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			Blend SrcAlpha OneMinusSrcAlpha

			LOD 100
			ZWrite Off


			Pass
			{
				Stencil
				{
					Ref[_StencilRef]
					Comp equal
					Pass keep
				}

				CGPROGRAM
				#pragma vertex vert alpha
				#pragma fragment frag alpha
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = _Color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
			return col;
		}
		ENDCG
	}
		}
}
