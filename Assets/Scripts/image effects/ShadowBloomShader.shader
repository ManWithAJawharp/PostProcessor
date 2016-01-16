Shader "Argia & Iluna/Image Effects/Shadow Bloom"
{
	Properties
	{
		[HideInInspector]_MainTex("Render Input", 2D) = "white" {}[HideInInspector]
		[HideInInspector]_ShadowMask("Render Input", 2D) = "black" {}
		_LUT("Lookup Table", 2D) = "black" {}
		_Iterations("Blur Iterations", Int) = 20
		_Offset("Blur Size", Float) = 1
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	#include "AutoLight.cginc"

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
			uniform sampler2D _ShadowMask;
			uniform int _Iterations;
			uniform half _Offset;
			//uniform sampler2D _ShadowMapTexture;

			float4 frag(v2f_img i) : SV_Target
			{
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					i.uv.y = 1 - i.uv.y;
				#endif

				float sum = tex2D(_ShadowMask, i.uv);
				half offset = _Offset;
				half2 uvOffset;

				half pi = 3.14159265359;

				int iter = _Iterations;

				for (int n = 1; n < iter; n++)
				{
					uvOffset = half2(_MainTex_TexelSize.x * offset, 0);

					sum += tex2D(_ShadowMask, i.uv + uvOffset * n);
					sum += tex2D(_ShadowMask, i.uv - uvOffset * n);
				}

				sum /= 2 * (iter - 1) + 1;

				float mask = tex2D(_ShadowMask, i.uv);

				return float4(sum, 0, 0, mask);
				//return mask;
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
			uniform sampler2D _LUT;
			uniform sampler2D _GrabTexture : register(s0);
			uniform int _Iterations;
			uniform half _Offset;

			float4 frag(v2f_img i) : SV_Target
			{
				float sum = tex2D(_GrabTexture, i.uv);
				half offset = _Offset;
				half2 uvOffset;

				half pi = 3.14159265359;

				int iter = _Iterations;

				for (int n = 1; n < iter; n++)
				{
					uvOffset = half2(0, _MainTex_TexelSize.x * offset);

					sum += tex2D(_GrabTexture, i.uv + uvOffset * n).x;
					sum += tex2D(_GrabTexture, i.uv - uvOffset * n).x;
				}

				sum /= 2 * (iter - 1) + 1;

				half factor = (cos(_Time.y * 5) + 49) / 50;

				sum *= factor;

				//Overlay layers
				float4 a = tex2D(_LUT, half2(clamp(sum, 0, 1), 0.5));
				float4 b = tex2D(_MainTex, i.uv);

				return step(0.5, a) * 2 * a * b + step(a, 0.5) * (1 - 2 * (1 - a) * (1 - b));
				//return tex2D(_GrabTexture, i.uv).w;
			}

			ENDCG
		}
	}
}
