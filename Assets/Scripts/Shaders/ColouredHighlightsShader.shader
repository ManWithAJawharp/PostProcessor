Shader "Argia-Iluna/ColouredHighlightsShader"
{
	Properties
	{
		_MainTex("Color", 2D) = "white" {}
		_Light("Light Colour", Color) = (1, 1, 0, 1)
		_Shadow("Shadow Colour", Color) = (0, 0, 1, 1)
		_Spread("Highlight Spreading", float) = 2
		_Intensity("HighLight Intensity", float) = 1
	}

		SubShader
	{
		Pass
		{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _Light;
			uniform float4 _Shadow;
			uniform half _Spread;
			uniform half _Intensity;

			struct v2f
			{
				float4 position: SV_POSITION;
				float3 normal : NORMAL;
				half2 uv : TEXCOORDS0;
			};

			v2f vert(appdata_base i)
			{
				v2f o;

				o.position = mul(UNITY_MATRIX_MVP, i.vertex);
				o.normal = normalize(i.normal);
				o.uv = i.texcoord;

				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

				float3 lightHL = pow(clamp(dot(lightDirection, i.normal), 0, 1), 1 / _Spread) * _Light * _Intensity;
				float3 shadowHL = pow(clamp(dot(-lightDirection, i.normal), 0, 1), 1 / _Spread) * _Shadow * _Intensity;
				float3 overlay = lightHL + shadowHL;

				float3 tex = tex2D(_MainTex, i.uv);

				return float4(step(0.5, overlay) * 2 * overlay * tex + step(overlay, 0.5) * (1 - 2 * (1 - overlay) * (1 - tex)), 0);
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
