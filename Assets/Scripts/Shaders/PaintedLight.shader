Shader "Argia & Iluna/Shaders/PaintedLight" {
	Properties {
		_MainTex("Main Texture (rgb)", 2D) = "white" {}
		_LUT("Lookup Table", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _LUT;

			struct v2f {
				float4 position: SV_Position;
				float3 normal : NORMAL;
				half2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base i)
			{
				v2f o;

				o.position = mul(UNITY_MATRIX_MVP, i.vertex);
				o.normal = normalize(mul(i.normal, _World2Object));
				o.uv = i.texcoord;

				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float lightIntensity;
				float4 color = tex2D(_MainTex, i.uv);

				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

				lightIntensity = dot(lightDirection, i.normal);

				float3 perp = normalize(float3(lightDirection.y, -lightDirection.x, lightDirection.z));
				float y = dot(perp, i.normal) / 2 + 0.5;

				float3 light = tex2D(_LUT, half2(lightIntensity / 2 + 0.5, y)).rgb;

				float4 finalColor;
				float3 a = light;
				float3 b = color;
				finalColor.rgb = step(a, 0.5) * (2 * b * a + pow(b, 2) * (1 - 2 * a)) +	//	Color layer.
					step(0.5, a) * (2 * b * (1 - a) + pow(b, 0.5) * (2 * a - 1));	//	Lighting layer.
				finalColor.a = 1;

				//return tex2D(_LUT, half2(lightIntensity / 2 + 0.5, 0)) * (lightIntensity / 2 + 1) * tex * UNITY_LIGHTMODEL_AMBIENT * 4;
				return finalColor;
			}

			ENDCG
		}
	}
}
