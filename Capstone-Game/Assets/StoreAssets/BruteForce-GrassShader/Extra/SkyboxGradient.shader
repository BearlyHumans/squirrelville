// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skybox/GradientSkybox"
{
	Properties
	{
		_Color1("TopColor", Color) = (1, 1, 1, 1)
		_Color2("HorizonColor", Color) = (1, 1, 1, 1)
		_Color3("BottomColor", Color) = (1, 1, 1, 1)
		_Intensity1("IntensityTop", Float) = 1.0		
		_Intensity2("IntensityMid", Float) = 1.0
		_Intensity3("IntensityBot", Float) = 1.0
	}

		CGINCLUDE

#include "UnityCG.cginc"

		struct appdata
	{
		float4 position : POSITION;
		float3 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float3 texcoord : TEXCOORD0;
	};

	half4 _Color1,_Color3,_Color2;
	half _Intensity1, _Intensity2, _Intensity3;

	v2f vert(appdata v)
	{
		v2f o;
		o.position = UnityObjectToClipPos(v.position);
		o.texcoord = v.texcoord;
		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		float p = normalize(i.texcoord).y;
		float p1 = 1.0f - pow(min(1.0f, 1.0f - p), _Intensity1);
		float p3 = 1.0f - pow(min(1.0f, 1.0f + p), _Intensity3);
		float p2 = 1.0f - p1 - p3;
		return (_Color1 * p1 + _Color2 * p2 + _Color3 * p3) * _Intensity2;
	}

		ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Background" "Queue" = "Background" }
			Pass
		{
			ZWrite Off
			Cull Off
			Fog { Mode Off }
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}