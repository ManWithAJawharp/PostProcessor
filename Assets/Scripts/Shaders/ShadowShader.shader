Shader "Argia & Iluna/Shadows/Shadow Shader"
{
	Properties
	{
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}
		_Fallof("Texture Fallof", Range(0, 50)) = 1
		_Color("Main Color", Color) = (1,0,0.5,0.5)
		_Transparency("Transparency", Range(0, 1)) = 0.2
		
	}

		SubShader
		{
			Tags{ "Queue" = "Transparent" "RenderType" = "Shadow" }
			Blend SrcAlpha OneMinusSrcAlpha
			Cull front

			CGPROGRAM

			#pragma surface surf NoLighting noforwardadd noshadow alpha
			//#pragma surface surf Lambert alpha

			sampler2D _MainTex;
			fixed3 _Color;
			half _Transparency;
			half _Fallof;

			struct Input
			{
				half2 uv_MainTex;
			};

			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
			{
				fixed4 c;

				c.rgb = s.Albedo;
				c.a = s.Alpha;

				return c;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);

				//o.Albedo = 1;
				//o.Alpha = 0;
				
				//o.Albedo = ((1 - tex.rgb) * _Color - 0.5) * 0.5 + 0.5;
				o.Albedo = pow(1 - tex.rgb, _Fallof) * _Color;
				o.Alpha = pow(1 - tex.a, _Fallof) * (1 - _Transparency);
			}

			ENDCG
		}

	//Fallback "Diffuse"
}
