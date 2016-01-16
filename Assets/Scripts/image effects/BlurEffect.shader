Shader "Argia & Iluna/Image Effects/Blur Effect"
{
	Properties
	{
		[HideInInspector]_MainTex("Render Input", 2D) = "white" {}
		_Iterations("Blur Iterations", Int) = 10
		_Offset("Blur Size", Float) = 5
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			uniform sampler2D _MainTex;
			uniform half2 _MainTex_TexelSize;
			uniform int _Iterations;
			uniform half _Offset;

			float4 frag(v2f_img i) : SV_Target
			{
				float4 sum = 0;
				half offset = _Offset;
				half2 uvOffset;

				half pi = 3.14159265359;

				int iter = _Iterations;

				for (int n = 1; n < iter; n++)
				{
					uvOffset = half2(_MainTex_TexelSize.x * offset, 0);

					sum += tex2D(_MainTex, i.uv + uvOffset * n)/* * - (cos(pi * n / iter) - 1) / 2*/;
					sum += tex2D(_MainTex, i.uv - uvOffset * n)/* * - (cos(pi * n / iter) - 1) / 2*/;
				}

				sum += tex2D(_MainTex, i.uv);

				sum /= 2 * iter + 1;

				return sum;
			}

			ENDCG
		}

		GrabPass{}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform sampler2D _GrabTexture : register(s0);
			uniform int _Iterations;
			uniform half _Offset;

			float4 frag(v2f_img i) : SV_Target
			{
				float4 sum = 0;
				half offset = _Offset;
				half2 uvOffset;

				half pi = 3.14159265359;

				int iter = _Iterations;

				for (uint n = 1; n < iter; n++)
				{
					uvOffset = half2(0, _MainTex_TexelSize.y * offset);

					sum += tex2D(_GrabTexture, i.uv + uvOffset * n)/* * -(cos(pi * n / iter) - 1) / 2*/;
					sum += tex2D(_GrabTexture, i.uv - uvOffset * n)/* * -(cos(pi * n / iter) - 1) / 2*/;
				}

				sum += tex2D(_GrabTexture, i.uv);

				sum /= 2 * iter + 1;

				//return (tex2D(_MainTex, i.uv) + sum) / 2;
				return sum;
			}

			ENDCG
		}
	}
}
