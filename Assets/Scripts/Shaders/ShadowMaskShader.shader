Shader "Argia & Iluna/Shadows/Shadow Mask"
{
	Properties
	{
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Player" }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				fixed4 position : SV_POSITION;
			};

			v2f vert(appdata i)
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

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
			};

			v2f vert(appdata i)
			{
				v2f o;

				o.position = mul(UNITY_MATRIX_MVP, i.vertex);

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return 0;
			}

			ENDCG
		}
	}

	Fallback "Diffuse"
}
