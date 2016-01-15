Shader "Hidden/Light ReplaceShader"
{
	SubShader
	{
		Tags { "RenderType" = "Player" }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				fixed4 vertex : POSITION;
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
				return fixed4(1, 0, 0, 0);
			}
				
			ENDCG
		}
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Opaque" }

		Blend SrcColor DstColor

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				fixed4 vertex : POSITION;
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
				return fixed4(0, 1, 0, 0);
			}

			ENDCG
		}
	}
}
