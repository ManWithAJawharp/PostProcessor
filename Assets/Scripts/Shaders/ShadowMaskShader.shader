Shader "Argia & Iluna/Shadows/Shadow Mask"
{
	Properties
	{
	}

	SubShader
	{
		ZWrite On
		Tags{ "Queue" = "Overlay" "RenderType" = "Player" }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				fixed4 position : SV_POSITION;
			};

			v2f vert(appdata_base i)
			{
				v2f o;

				o.position = mul(UNITY_MATRIX_MVP, i.vertex);

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return 1;
			}
				
			ENDCG
		}
	}

	Fallback "Diffuse"

	SubShader
	{
		ZWrite On
		Tags{ "Queue" = "Overlay" "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 position : SV_POSITION;
			};

			v2f vert(appdata_base i)
			{
				v2f o;

				o.position = mul(UNITY_MATRIX_MVP, i.vertex);

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return 0.5;
			}

			ENDCG
		}
	}

	Fallback "Diffuse"
}
