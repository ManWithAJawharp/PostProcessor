Shader "Argia & Iluna/Image Effects/Object Picker"
{
	Properties
	{
		[HideInInspector]_MainTex("Render Input", 2D) = "white" {}
		[HideInInspector]_ShadowMask("Shadow Mask Input", 2D) = "black" {}
		_LightColor("Light Color", Color) = (1, 1, 0, 0)
		_ShadowColor("Shadow Color", Color) = (0, 1, 1, 0)
		_LUT("Look Up Table", 2D) = "white" {}
		_Iterations("Bloom Iterations", int) = 10
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off

		Pass	//	0;
		{
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			uniform int _Iterations;

			float4 frag(v2f_img i) : SV_Target
			{
				UNITY_DECLARE_SHADOWMAP(shadow);

				float mask = tex2D(_MainTex, i.uv).r;

				float bloom = 0;
				float invBloom = 0;

				half2 offset;
				int iterations = clamp(_Iterations, 0, 10);
				//iterations = 5;
				int reach = 8;

				for (int n = 1; n < iterations; n++)
				{
					offset = half2(reach * n / _MainTex_TexelSize.z + sin(i.pos.x) * _MainTex_TexelSize.x, 0);

					bloom += (tex2D(_MainTex, i.uv + offset)	//	Left
						+ tex2D(_MainTex, i.uv - offset)) / iterations / 1.2;	//	Right

					invBloom += (tex2D(_MainTex, i.uv - offset)
						+ tex2D(_MainTex, i.uv + offset)) / iterations / 1.4;
				}

				bloom = clamp(bloom + mask, 0, 1) * (1 - mask);
				invBloom = clamp(invBloom - mask + 0.6, 0, 1) * mask;
				
				return float4(bloom + mask, invBloom, 0, mask);
			}

			ENDCG
		}

		Pass	//	1;
		{
			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 3.0

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			uniform sampler2D _ShadowMask;
			uniform float4 _LightColor;
			uniform float4 _ShadowColor;
			uniform sampler2D _LUT;
			uniform int _Iterations;

			float4 frag(v2f_img i) : SV_Target
			{
				float4 color = tex2D(_MainTex, i.uv);

				//	Flip the uv's for the RenderTexture
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					i.uv.y = 1 - i.uv.y;
				#endif

				float4 mask = tex2D(_ShadowMask, i.uv);

				float bloom = 0;
				float invBloom = 0;

				half2 offset;
				int iterations = clamp(_Iterations, 0, 10);
				//iterations = 5;
				int reach = 8;

				for (int n = 1; n < iterations; n++)
				{
					offset = half2(0, reach * n / _MainTex_TexelSize.w + sin(i.pos.y) * _MainTex_TexelSize.x);

					bloom += (tex2D(_ShadowMask, i.uv + offset).r	//	Top
						+ tex2D(_ShadowMask, i.uv - offset).r) / iterations / 1.2;	//	Bottom

					invBloom += ((tex2D(_ShadowMask, i.uv - offset).g + mask.a)
						+ (tex2D(_ShadowMask, i.uv + offset).g + mask.a) / 2) / iterations / 2;
				}

				bloom = clamp(bloom, 0, 1);
				invBloom = clamp(invBloom, 0, 1);

				float4 overlay = tex2D(_LUT, half2(bloom + invBloom, 0.5));
				//return clamp(2 * color * overlay, 0, 1) * (1 - mask.a);
				//return tex2D(_LUT, half2(bloom, 0.25)) - mask.a;
				//return tex2D(_LUT, half2(invBloom, 0.75)) - (1 - mask.a);
				return bloom - mask.a;
			}

			ENDCG
		}
	}
}
