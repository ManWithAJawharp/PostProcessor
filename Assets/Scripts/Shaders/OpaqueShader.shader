Shader "Argia & Iluna/Shadows/Opaque Shader"
{
	Properties
	{
		_Color("Main Color", Color) = (0.5, 0.5, 0.5, 1)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		
		/*CGPROGRAM

		#pragma surface surf Lambert

		uniform float4 _Color;

		struct Input
		{
			float4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = _Color;
		}

		ENDCG*/

		CGPROGRAM

		#pragma surface surf CSLambert

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		half4 LightingCSLambert(SurfaceOutput s, half3 lightDir, half atten)
		{
			fixed diff = max(0, dot(s.Normal, lightDir));

			fixed4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);

			//shadow colorization
			c.rgb += float3(1, 0, 0) * (1.0 - atten);

			//c.rgb = 1 - atten;

			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		ENDCG

		/*Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 position : SV_POSITION;
				float3 normal : NORMAL;
				float3 lightDir : TEXCOORD0;
			};

			v2f vert(appdata_base i)
			{
				v2f o;

				o.position = mul(UNITY_MATRIX_MVP, i.vertex);
				o.normal = i.normal;

				return o;
			}

			float3 frag(v2f i) : COLOR
			{
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

				return step(0.5, - dot(lightDirection, i.normal));
			}

			ENDCG
		}*/
	}
	Fallback "Diffuse"
}
