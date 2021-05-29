// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/VertexColor"
{
	Properties
	{
		_ColorLight("SunColor", Color) = (1,1,1,1)
		_SunForce("SunForce",float) = 1
		_VertexColorForce("VertexColorForce",float) = 1
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		//LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert nofog
			#pragma fragment frag

			#include "UnityCG.cginc"

			half4 _ColorLight;
			float _SunForce, _VertexColorForce;

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				fixed4 color: texcoord0;
				float4 vertex : SV_POSITION;
			};
         
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
            o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _ColorLight*_SunForce * clamp((i.color+_VertexColorForce),0,1);
			}
			ENDCG
		}
	}
}
