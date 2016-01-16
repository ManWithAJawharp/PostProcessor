Shader "Argia & Iluna/Image Effects/Edge Blur"
{
	Properties
	{
		_MainTex("Render Input", 2D) = "white" {}
		_ShadowMask("Render Input", 2D) = "black" {}
		_LUT("Lookup Table", 2D) = "black" {}
		_Light("Light Color", Color) = (1, 1, 0, 1)
		_LightGrad("Light Gradient Color", Color) = (1, 0, 0, 0)
		_Shadow("Shadow Color", Color) = (1, 1, 0, 1)
		_ShadowGrad("Shadow Gradient Color", Color) = (1, 0, 0, 0)
		_Iterations("Blur Iterations", Int) = 7
		_Offset("Blur Size", Float) = 6
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

			float4 frag(v2f_img i) : SV_Target
			{
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					i.uv.y = 1 - i.uv.y;
				#endif

				float sum = tex2D(_ShadowMask, i.uv);
				half offset = _Offset;
				half2 uvOffset = half2(_MainTex_TexelSize.x * offset, 0);

				half pi = 3.14159265359;

				int iter = _Iterations;

				for (int n = 1; n < iter; n++)
				{
					sum += tex2D(_ShadowMask, i.uv + uvOffset * n).x;
					sum += tex2D(_ShadowMask, i.uv - uvOffset * n).x;
				}

				sum /= 2 * (iter - 1) + 1;

				float mask = tex2D(_ShadowMask, i.uv);

				//return tex2D(_ShadowMask, i.uv);
				return float4(sum, 0, 0, mask);
				//return tex2D(_ShadowMask, i.uv);
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
			uniform half4 _Light;
			uniform half4 _LightGrad;
			uniform half4 _ShadowGrad;
			uniform half4 _Shadow;
			uniform sampler2D _GrabTexture : register(s0);
			uniform int _Iterations;
			uniform half _Offset;

			fixed4 frag(v2f_img i) : SV_Target
			{
				float sum = tex2D(_GrabTexture, i.uv).x;
				half offset = _Offset;
				half2 uvOffset = half2(0, _MainTex_TexelSize.y * offset);

				half pi = 3.14159265359;

				int iter = _Iterations;

				for (int n = 1; n < iter; n++)
				{
					sum += tex2D(_GrabTexture, i.uv + uvOffset * n).x;
					sum += tex2D(_GrabTexture, i.uv - uvOffset * n).x;
				}

				sum /= 2 * (iter - 1) + 1;

				half factor = (cos(_Time.y * 5) + 49) / 50;

				sum *= factor;

				half mask = tex2D(_GrabTexture, i.uv).w;

				half light = clamp(1 - sum - mask, 0, 1);
				half lightG = clamp(sum - mask, 0, 1);
				half shadowG = clamp(mask -  sum, 0, 1);
				half shadow = clamp(sum + mask - 1, 0, 1);

				//Overlay layers
				half4 a = _Light * light + _LightGrad * lightG + _ShadowGrad * shadowG +_Shadow * shadow;
				half4 b = tex2D(_MainTex, i.uv);

				return step(0.5, a) * 2 * a * b + step(a, 0.5) * (1 - 2 * (1 - a) * (1 - b));
				//return tex2D(_GrabTexture, i.uv);
			}

			ENDCG
		}
	}
}
