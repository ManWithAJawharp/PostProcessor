Shader "Hidden/ChromaticAberration"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Intensity ("Intensity", Float) = 0
		_Iterations ("Iterations", Int) = 8
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform sampler2D _CameraDepthTexture;

			struct appdata
			{
				fixed4 vertex : POSITION;
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				fixed4 projPos : TEXCOORD1;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.projPos = ComputeScreenPos(o.pos);
				return o;
			}
			
			sampler2D _MainTex;
			half _Intensity;
			int _Iterations;

			fixed4 frag (v2f i) : SV_Target
			{
				half depth = clamp(50 * Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r) - 0.2, 0, 1);

				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 blur = 0;

				half intensity = _Intensity * (sin(_Time.y * 8) + 1) * 0.5;

				for (int j = 0; j < _Iterations; j++)
				{
					//blur += tex2D(_MainTex, i.uv + depth * half2(j * 0.002, 0));
					//blur += tex2D(_MainTex, i.uv - depth * half2(j * 0.002, 0));

					fixed red = tex2D(_MainTex, (0.5 - i.uv) * -depth *_Intensity * j / 128 + i.uv).r;
					fixed green = tex2D(_MainTex, (0.5 - i.uv) *_Intensity * j / 128 + i.uv).g;
					fixed blue = tex2D(_MainTex, (0.5 - i.uv) * depth *_Intensity * j / 128 + i.uv).b;

					blur += fixed4(red, green, blue, 0);
				}

				return blur / _Iterations;

				//half intensity = _Intensity * (sin(_Time.y * 8) + 1) * 0.5;

				//half2 aberration1 = (0.5 - i.uv) * intensity + i.uv;
				//half2 aberration2 = (0.5 - i.uv) * - intensity + i.uv;

				//fixed red = tex2D(_MainTex, aberration1).r;
				//fixed green = tex2D(_MainTex, i.uv).g;
				//fixed blue = tex2D(_MainTex, aberration2).b;

				//return fixed4(red, green, blue, 0);
				//return depth;
			}
			ENDCG
		}
	}
}
