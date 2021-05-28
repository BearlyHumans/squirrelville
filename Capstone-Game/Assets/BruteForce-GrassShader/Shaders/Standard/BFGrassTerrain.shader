// MADE BY MATTHIEU HOULLIER
// Copyright 2021 BRUTE FORCE, all rights reserved.
// You are authorized to use this work if you have purchased the asset.
// Mail me at bruteforcegamesstudio@gmail.com if you have any questions or improvements you want.
Shader "BruteForce/InteractiveGrassTerrain"
{
	Properties
	{
		// Terrain properties //
		[HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
	    // Textures
		[HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}
		[HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
		
		// Normal Maps
		[HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
		[HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}

		// specs color
		[HideInInspector] _Specular0("Specular 0 (R)", Color) = (1,1,1,1)
		[HideInInspector] _Specular1("Specular 1 (G)", Color) = (1,1,1,1)
		[HideInInspector] _Specular2("Specular 2 (B)", Color) = (1,1,1,1)
		[HideInInspector] _Specular3("Specular 3 (A)", Color) = (1,1,1,1)

		[HideInInspector] _Splat0_ST("Size0", Vector) = (1,1,0)
		[HideInInspector] _Splat1_ST("Size1", Vector) = (1,1,0)
		[HideInInspector] _Splat2_ST("Size2", Vector) = (1,1,0)
		[HideInInspector] _Splat3_ST("Size3", Vector) = (1,1,0)

		[Header(Tint Colors)]
		[Space]
		[MainColor]_Color("Tint Color",Color) = (0.5 ,0.5 ,0.5,1.0)
		_GroundColor("Tint Ground Color",Color) = (0.7 ,0.68 ,0.68,1.0)
		_SelfShadowColor("Shadow Color",Color) = (0.41 ,0.41 ,0.36,1.0)
		_ProjectedShadowColor("Projected Shadow Color",Color) = (0.45 ,0.42 ,0.04,1.0)
		_GrassShading("Grass Shading", Range(0.0, 1)) = 0.197
		_GrassSaturation("Grass Saturation", Float) = 2

		[Header(Textures)]
		[Space]
		[MainTexture]_MainTex("Color Grass", 2D) = "white" {}
		[NoScaleOffset]_GroundTex("Ground Texture", 2D) = "white" {}
		[NoScaleOffset]_NoGrassTex("No-Grass Texture", 2D) = "white" {}
		[NoScaleOffset]_GrassTex("Grass Pattern", 2D) = "white" {}
		[NoScaleOffset]_Noise("Noise Color", 2D) = "white" {}
		[NoScaleOffset]_Distortion("Distortion Wind", 2D) = "white" {}

		[Header(Geometry Values)]
		[Space]
		_NumberOfStacks("Number Of Stacks", Range(0, 17)) = 12
		_OffsetValue("Offset Normal", Float) = 1
		_OffsetVector("Offset Vector", Vector) = (0,0,0)
		_FadeDistanceStart("Fade-Distance Start", Float) = 16
		_FadeDistanceEnd("Fade-Distance End", Float) = 26
		_MinimumNumberStacks("Minimum Number Of Stacks", Range(0, 17)) = 2

		[Header(Rim Lighting)]
		[Space]
		_RimColor("Rim Color", Color) = (0.14, 0.18, 0.09, 1)
		_RimPower("Rim Power", Range(0.0, 8.0)) = 3.14
		_RimMin("Rim Min", Range(0,1)) = 0.241
		_RimMax("Rim Max", Range(0,1)) = 0.62

		[Header(Grass Values)]
		[Space]
		_GrassThinness("Grass Thinness", Range(0.01, 2)) = 0.4
		_GrassThinnessIntersection("Grass Thinness Intersection", Range(0.01, 2)) = 0.43
		_TilingN1("Tiling Of Grass", Float) = 6.06
		_WindMovement("Wind Movement Speed", Float) = 0.55
		_WindForce("Wind Force", Float) = 0.35
		_TilingN3("Wind Noise Tiling", Float) = 1
		_GrassCut("Grass Cut", Range(0, 1)) = 0
		_TilingN2("Tiling Of Noise Color", Float) = 0.05
		_NoisePower("Noise Power", Float) = 2
		[Toggle]_RTEffect("RenderTexture Effect", Int) = 1

		[Header(Terrain)]
		[Space]
		[Toggle]_UseBiplanar("Use Biplanar", Int) = 0
		_BiPlanarStrength("BiPlanarStrength", Float) = 1
		_BiPlanarSize("BiPlanarSize", Float) = 1
	}
		SubShader
		{
			pass 
			{
			Tags{ "LightMode" = "ForwardBase" "DisableBatching" = "true" }
			LOD 200
			ZWrite true
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase

			#define SHADOWS_SCREEN
			#include "AutoLight.cginc"
			//#include "Lighting.cginc"
			#include "UnityCG.cginc"
			#pragma multi_compile _ LIGHTMAP_ON
			uniform float4 _LightColor0;
			uniform sampler2D _LightTexture0;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
#ifdef LIGHTMAP_ON
					half4 texcoord1 : TEXCOORD1;
#endif
			};

			struct v2g
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 objPos : TEXCOORD1;
				float3 normal : TEXCOORD2;
				SHADOW_COORDS(4)
				UNITY_FOG_COORDS(5)
				UNITY_VERTEX_INPUT_INSTANCE_ID
#ifdef LIGHTMAP_ON
					float2 lmap : TEXCOORD6;
#endif
			};

			struct g2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				half1 color : TEXCOORD2;
				float3 normal : TEXCOORD3;
				SHADOW_COORDS(4)
				UNITY_FOG_COORDS(5)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
#ifdef LIGHTMAP_ON
					float2 lmap : TEXCOORD6;
#endif
			};

			struct SHADOW_VERTEX // This is needed for custom shadow casting
			{
				float4 vertex : POSITION;
			};
			// Render Texture Effects //
			uniform sampler2D _GlobalEffectRT;
			uniform float3 _Position;
			uniform float _OrthographicCamSize;
			uniform sampler2D _Control;
			uniform float _HasRT;

			int _NumberOfStacks, _RTEffect, _MinimumNumberStacks, _UseBiplanar;
			sampler2D _MainTex;
			sampler2D _NoGrassTex;
			float4 _MainTex_ST;
			sampler2D _Distortion;
			sampler2D _GrassTex;
			sampler2D _Noise;
			sampler2D _GroundTex;
			float _TilingN1;
			float _TilingN2, _WindForce;
			float4 _Color, _SelfShadowColor, _GroundColor, _ProjectedShadowColor;
			float4 _OffsetVector;
			float _TilingN3, _BiPlanarStrength, _BiPlanarSize;
			float _WindMovement, _OffsetValue;
			half _GrassThinness, _GrassShading, _GrassThinnessIntersection, _GrassCut;
			half4 _RimColor;
			half _RimPower, _NoisePower, _GrassSaturation, _FadeDistanceStart, _FadeDistanceEnd;
			half _RimMin, _RimMax;
			half4 _Specular0, _Specular1, _Specular2, _Specular3;
			float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
			sampler2D _Splat0, _Splat1, _Splat2, _Splat3, _Normal0, _Normal1, _Normal2, _Normal3;
			v2g vert(appdata v)
			{
				v2g o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.objPos = v.vertex;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o._ShadowCoord = ComputeScreenPos(o.pos);
				o.normal = v.normal;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
#ifdef LIGHTMAP_ON
				o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			#define UnityObjectToWorld(o) mul(unity_ObjectToWorld, float4(o.xyz,1.0))
			[maxvertexcount(51)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> tristream) 
			{
				g2f o;
				SHADOW_VERTEX v;

				_OffsetValue *= 0.01;
				// Loop 3 times for the base ground geometry
				for (int i = 0; i < 3; i++)
				{
					UNITY_SETUP_INSTANCE_ID(input[i]);
					UNITY_TRANSFER_INSTANCE_ID(input[i], o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.uv = input[i].uv;
					o.pos = input[i].pos;
					o.color = 0.0 + _GrassCut;
					o.normal = normalize(mul(float4(input[i].normal, 0.0), unity_WorldToObject).xyz);
					o.worldPos = UnityObjectToWorld(input[i].objPos);
					o._ShadowCoord = input[i]._ShadowCoord;
					UNITY_TRANSFER_FOG(o, o.pos);
#ifdef LIGHTMAP_ON
					o.lmap = input[i].lmap.xy;
#endif
					tristream.Append(o);
				}
				tristream.RestartStrip();

				float dist = distance(_WorldSpaceCameraPos, UnityObjectToWorld((input[0].objPos / 3 + input[1].objPos / 3 + input[2].objPos / 3)));
				if (dist > 0)
				{
					int NumStacks = lerp(_NumberOfStacks + 1, 0, (dist - _FadeDistanceStart)*(1 / max(_FadeDistanceEnd - _FadeDistanceStart, 0.0001)));//Clamp because people will start dividing by 0
					_NumberOfStacks = min(clamp(NumStacks, clamp(_MinimumNumberStacks, 0, _NumberOfStacks), 17), _NumberOfStacks);
				}

				float4 P; // P is shadow coords new position
				float4 objSpace; // objSpace is the vertex new position
				// Loop 3 times * numbersOfStacks for the grass
					for (float i = 1; i <= _NumberOfStacks; i++) 
					{
						float4 offsetNormal = _OffsetVector * i*0.01;
						for (int ii = 0; ii < 3; ii++)
						{
							UNITY_SETUP_INSTANCE_ID(input[ii]);
							UNITY_TRANSFER_INSTANCE_ID(input[ii], o);
							UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
#ifdef LIGHTMAP_ON
							o.lmap = input[ii].lmap.xy;
#endif
							float thicknessModifier = 1;
							float dist = distance(_WorldSpaceCameraPos, UnityObjectToWorld(input[ii].objPos));
							if (dist > 0)
							{
								thicknessModifier = lerp(0.1*0.01, _OffsetValue, (dist - 1)*(1 / max(3 - 1, 0.0001)));//Clamp because people will start dividing by 0
								thicknessModifier = clamp(thicknessModifier, 0.1*0.01, _OffsetValue);
							}

							P = input[ii]._ShadowCoord + _OffsetVector * _NumberOfStacks*0.01;
							float4 NewNormal = float4(normalize(input[ii].normal),0);
							objSpace = float4(input[ii].objPos + NewNormal * thicknessModifier*i + offsetNormal);
							o.color = (i / (_NumberOfStacks - _GrassCut));
							o.uv = input[ii].uv;
							o.pos = UnityObjectToClipPos(objSpace);
							o._ShadowCoord = P;
							o.worldPos = UnityObjectToWorld(objSpace);
							o.normal = normalize(mul(float4(input[ii].normal, 0.0), unity_WorldToObject).xyz);

							v.vertex = mul(unity_WorldToObject, UnityObjectToWorld(objSpace));
							TRANSFER_VERTEX_TO_FRAGMENT(o);
							UNITY_TRANSFER_FOG(o, o.pos);
							tristream.Append(o);
						}
						tristream.RestartStrip();
				}
			}
			half4 frag(g2f i) : SV_Target
			{
				float2 uv = i.worldPos.xz - _Position.xz;
				uv = uv / (_OrthographicCamSize * 2);
				uv += 0.5;

				float bRipple = 1;

				if (_HasRT)
				{
					if (_RTEffect == 1)
					{
						bRipple = 1 - clamp(tex2D(_GlobalEffectRT, uv).b * 5, 0, 2);
					}
				}

				float2 dis = tex2D(_Distortion, i.uv  *_TilingN3 + _Time.xx * 3 * _WindMovement);
				float displacementStrengh = 0.6* (((sin(_Time.y + dis * 5) + sin(_Time.y*0.5 + 1.051)) / 5.0) + 0.15*dis)*bRipple; //hmm math
				dis = dis * displacementStrengh*(i.color.r*1.3)*_WindForce*bRipple;

				float ripples = 0.25;
				float ripples2 = 0;
				float ripplesG = 0;
				float ripples3 = 0;

				if (_HasRT)
				{
					if (_RTEffect == 1)
					{
						// .b(lue) = Grass height / .r(ed) = Grass shadow / .g(reen) = Remove Grass
						ripples = (0.25 - tex2D(_GlobalEffectRT, uv + dis.xy*0.04).b);
						ripples2 = (tex2D(_GlobalEffectRT, uv + dis.xy*0.04).r);
						ripplesG = (0 - tex2D(_GlobalEffectRT, uv + dis.xy*0.04).g);
						ripples3 = (0 - ripples2)*ripples2;
					}
				}
				half4 splat_control = tex2D(_Control, i.uv + dis.xy*0.05);

				// SplatTexture //
				float3 grassPatternSplat0 = tex2D(_Splat0, i.uv*_TilingN1*_Splat0_ST.z + dis.xy);
				float3 grassPatternSplat1 = tex2D(_Splat1, i.uv*_TilingN1*_Splat1_ST.z + dis.xy);
				float3 grassPatternSplat2 = tex2D(_Splat2, i.uv*_TilingN1*_Splat2_ST.z + dis.xy);
				float3 grassPatternSplat3 = tex2D(_Splat3, i.uv*_TilingN1*_Splat3_ST.z + dis.xy);
				float3 colNormal0 = tex2D(_Normal0, i.uv*_Splat0_ST.z + dis.xy*0.09)*_Specular0;
				float3 colNormal1 = tex2D(_Normal1, i.uv*_Splat1_ST.z + dis.xy*0.09)*_Specular1;
				float3 colNormal2 = tex2D(_Normal2, i.uv*_Splat2_ST.z + dis.xy*0.09)*_Specular2;
				float3 colNormal3 = tex2D(_Normal3, i.uv*_Splat3_ST.z + dis.xy*0.09)*_Specular3;

				float3 normalDir = i.normal;
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				float rim = 1 - saturate(dot(viewDir, normalDir));
				float3 rimLight = pow(rim, _RimPower);
				rimLight = smoothstep(_RimMin, _RimMax, rimLight);

				half4 col = half4( lerp(grassPatternSplat0, colNormal1, splat_control.g),1);
				col.rgb = lerp(col.rgb, colNormal2, splat_control.b);
				col.rgb = lerp(col.rgb, colNormal3, splat_control.a);
				half4 colGround = tex2D(_Splat0, i.uv + dis.xy*0.05);

				float3 noise = tex2D(_Noise, i.uv*_TilingN2 + dis.xy)*_NoisePower;
				float3 grassPattern = lerp(grassPatternSplat0, grassPatternSplat1, splat_control.g);
				grassPattern = lerp(grassPattern, grassPatternSplat2, splat_control.b);
				grassPattern = lerp(grassPattern, grassPatternSplat3, splat_control.a);
				float GrassThinnessColor = lerp(_Splat0_ST.w, _Splat1_ST.w, splat_control.g);
				GrassThinnessColor = lerp(GrassThinnessColor, _Splat2_ST.w, splat_control.b);
				GrassThinnessColor = lerp(GrassThinnessColor, _Splat3_ST.w, splat_control.a)*_GrassThinness;
				
				half3 NoGrass = tex2D(_NoGrassTex, i.uv + dis.xy*0.05);
				NoGrass.r = saturate(NoGrass.r - splat_control.r);

				// Biplanar
				if (_UseBiplanar == 0)
				{
					//_BiPlanarSize = 1;
					//_BiPlanarStrength = 1;
				}
				else if (_UseBiplanar == 1)
				{
					float3 vec = mul(unity_ObjectToWorld, float4(i.normal, 0.0)).xyz;
					float threshold = smoothstep(_BiPlanarSize, _BiPlanarStrength, abs(dot(vec, float3(0, 1, 0))));
					NoGrass.r *= lerp(1, 0, threshold);
				}
				NoGrass.r = saturate(NoGrass.r + ripplesG);


				half alpha = step(1 - ((1+ grassPattern.x) * GrassThinnessColor)*((2 - i.color.r)*NoGrass.r*grassPattern.x)*saturate(ripples + 1)*saturate(ripples + 1), ((1 - i.color.r)*(ripples + 1))*(NoGrass.r*grassPattern.x)*GrassThinnessColor - dis.x * 5);
				alpha = lerp(alpha, alpha + (grassPattern.x*NoGrass.r*(1 - i.color.r))*_GrassThinnessIntersection ,1 - (NoGrass.r)*(ripples*NoGrass.r + 0.75));

				if (i.color.r >= 0.01)
				{
					if (alpha*(ripples3 + 1) - (i.color.r) < -0.02)discard;
				}
				_Color *= 2;
				col.xyz = (pow(col, _GrassSaturation) * _GrassSaturation)*float3(_Color.x, _Color.y, _Color.z);
				col.xyz *= saturate(lerp(_SelfShadowColor, 1, pow(i.color.x, 1.1)) + (_GrassShading  * (ripples * 1 + 1) - noise.x*dis.x * 2) - noise.x*dis.x * 2);
				col.xyz *= _Color*(ripples*-0.1 + 1);
				col.xyz *= 1 - (ripples2*(1 - saturate(i.color.r - 0.7)));

				if (i.color.r <= 0.01)
				{
					colGround.xyz *= _GroundColor * 2;
					col.xyz = lerp(colGround.xyz,col.xyz, NoGrass.r);
				}

				float shadowmap = LIGHT_ATTENUATION(i);
				half3 shadowmapColor = lerp(_ProjectedShadowColor,1,shadowmap);

				col.xyz += _RimColor.rgb * pow(rimLight, _RimPower);

				fixed3 lm = 1;
#ifdef LIGHTMAP_ON
				lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));
				col.rgb *= saturate(lm + 0.1);
#else
				col.xyz = col.xyz * saturate(shadowmapColor);
				if (_LightColor0.r + _LightColor0.g + _LightColor0.b > 0)
				{
					col.xyz *= _LightColor0;
				}
#endif				
				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
				//return splat_control;
			}
			ENDCG
		}

		// SHADOW CASTING PASS, this will redraw geometry so keep this pass disabled if you want to save performance
		// Keep it if you want depth for post process or if you're using deferred rendering
		Pass{
				Tags {"LightMode" = "ShadowCaster" "DisableBatching" = "true"  }
				//Tags{ "LightMode" = "ForwardBase" "DisableBatching" = "true" }
				//Tag for debugging only

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float4 normal : NORMAL;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2g
		{
			float2 uv : TEXCOORD0;
			float4 pos : SV_POSITION;
			float4 objPos : TEXCOORD1;
			float3 normal : TEXCOORD3;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct g2f
		{
			float2 uv : TEXCOORD0;
			float3 worldPos : TEXCOORD1;
			float3 normal : TEXCOORD3;
			float1 color : TEXCOORD2;
			V2F_SHADOW_CASTER;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};

		struct SHADOW_VERTEX
		{
			float4 vertex : POSITION;
		};

			sampler2D _MainTex;
			sampler2D _NoGrassTex;
			sampler2D _Noise;

			uniform sampler2D _GlobalEffectRT;
			uniform float3 _Position;
			uniform float _OrthographicCamSize;
			uniform sampler2D _Control;
			uniform float _HasRT;

			int _NumberOfStacks, _RTEffect, _MinimumNumberStacks, _UseBiplanar;
			float4 _MainTex_ST;
			sampler2D _Distortion;
			sampler2D _GrassTex;
			float _TilingN1;
			float _WindForce, _TilingN2;
			float4 _OffsetVector;
			float _TilingN3, _BiPlanarStrength, _BiPlanarSize;
			float _WindMovement, _OffsetValue, _FadeDistanceStart, _FadeDistanceEnd;
			half _GrassThinness, _GrassThinnessIntersection, _GrassCut, _NoisePower;			
			half4 _Specular0, _Specular1, _Specular2, _Specular3;
			float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
			sampler2D _Splat0, _Splat1, _Splat2, _Splat3, _Normal0, _Normal1, _Normal2, _Normal3;

					v2g vert(appdata v)
					{
						v2g o;
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_TRANSFER_INSTANCE_ID(v, o);
						o.objPos = v.vertex;
						o.pos = UnityObjectToClipPos(v.vertex);
						o.uv = TRANSFORM_TEX(v.uv, _MainTex);
						o.normal = v.normal;
						TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

						return o;
					}

					#define UnityObjectToWorld(o) mul(unity_ObjectToWorld, float4(o.xyz,1.0))
					[maxvertexcount(53)]
					void geom(triangle v2g input[3], inout TriangleStream<g2f> tristream) {

						g2f o;
						SHADOW_VERTEX v;

						_OffsetValue *= 0.01;

						for (int i = 0; i < 3; i++)
						{
							UNITY_SETUP_INSTANCE_ID(input[i]);
							UNITY_TRANSFER_INSTANCE_ID(input[i], o);
							UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
							o.uv = input[i].uv;
							o.pos = input[i].pos;
							o.color = float3(0 + _GrassCut, 0 + _GrassCut, 0 + _GrassCut);
							o.normal = normalize(mul(float4(input[i].normal, 0.0), unity_WorldToObject).xyz);
							o.worldPos = UnityObjectToWorld(input[i].objPos);

							tristream.Append(o);
						}
						float4 P;
						float4 objSpace;
						tristream.RestartStrip();

						float dist = distance(_WorldSpaceCameraPos, UnityObjectToWorld((input[0].objPos / 3 + input[1].objPos / 3 + input[2].objPos / 3)));
						if (dist > 0)
						{
							int NumStacks = lerp(_NumberOfStacks + 1, 0, (dist - _FadeDistanceStart)*(1 / max(_FadeDistanceEnd - _FadeDistanceStart, 0.0001)));//Clamp because people will start dividing by 0
							_NumberOfStacks = min(clamp(NumStacks, clamp(_MinimumNumberStacks, 0, _NumberOfStacks), 17), _NumberOfStacks);
						}

						for (float i = 1; i <= _NumberOfStacks; i++) 
						{
							float4 offsetNormal = _OffsetVector * i*0.01;
							for (int ii = 0; ii < 3; ii++)
							{
								UNITY_SETUP_INSTANCE_ID(input[ii]);
								UNITY_TRANSFER_INSTANCE_ID(input[ii], o);
								UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
								float thicknessModifier = 1;
								float dist = distance(_WorldSpaceCameraPos, UnityObjectToWorld(input[ii].objPos));
								if (dist > 0)
								{
									thicknessModifier = lerp(0.1*0.01, _OffsetValue, (dist - 1)*(1 / max(3 - 1, 0.0001)));//Clamp because people will start dividing by 0
									thicknessModifier = clamp(thicknessModifier, 0.1*0.01, _OffsetValue);
								}
								float4 NewNormal = float4(normalize(input[ii].normal), 0);
								objSpace = float4(input[ii].objPos + NewNormal * thicknessModifier*i + offsetNormal);
								o.color = (i / (_NumberOfStacks - _GrassCut));
								o.uv = input[ii].uv;
								o.pos = UnityObjectToClipPos(objSpace);
								o.worldPos = UnityObjectToWorld(objSpace);
								o.normal = normalize(mul(float4(input[ii].normal, 0.0), unity_WorldToObject).xyz);
								tristream.Append(o);
							}
							tristream.RestartStrip();
						}
					}

			float4 frag(g2f i) : SV_Target
			{
				float2 uv = i.worldPos.xz - _Position.xz;
				uv = uv / (_OrthographicCamSize * 2);
				uv += 0.5;

				float bRipple = 1;

				if (_HasRT)
				{
					if (_RTEffect == 1)
					{
						bRipple = 1 - clamp(tex2D(_GlobalEffectRT, uv).b * 5, 0, 2);
					}
				}
				float2 dis = tex2D(_Distortion, i.uv  *_TilingN3 + _Time.xx * 3 * _WindMovement);
				float displacementStrengh = 0.6* (((sin(_Time.y + dis * 5) + sin(_Time.y*0.5 + 1.051)) / 5.0) + 0.15*dis)*bRipple; //hmm math
				dis = dis * displacementStrengh*(i.color.r*1.3)*_WindForce*bRipple;

				float ripples = 0.25;
				float ripples2 = 0;
				float ripplesG = 0;
				float ripples3 = 0;

				if (_HasRT)
				{
					if (_RTEffect == 1)
					{
						// .b(lue) = Grass height / .r(ed) = Grass shadow / .g(reen) = Remove Grass
						ripples = (0.25 - tex2D(_GlobalEffectRT, uv + dis.xy*0.04).b);
						ripples2 = (tex2D(_GlobalEffectRT, uv + dis.xy*0.04).r);
						ripplesG = (0 - tex2D(_GlobalEffectRT, uv + dis.xy*0.04).g);
						ripples3 = (0 - ripples2)*ripples2;
					}
				}
				half4 splat_control = tex2D(_Control, i.uv + dis.xy*0.05);
				float3 grassPatternSplat0 = tex2D(_Splat0, i.uv*_TilingN1*_Splat0_ST.z + dis.xy);
				float3 grassPatternSplat1 = tex2D(_Splat1, i.uv*_TilingN1*_Splat1_ST.z + dis.xy);
				float3 grassPatternSplat2 = tex2D(_Splat2, i.uv*_TilingN1*_Splat2_ST.z + dis.xy);
				float3 grassPatternSplat3 = tex2D(_Splat3, i.uv*_TilingN1*_Splat3_ST.z + dis.xy);
				float3 colNormal0 = tex2D(_Normal0, i.uv*_Splat0_ST.z + dis.xy*0.09)*_Specular0;
				float3 colNormal1 = tex2D(_Normal1, i.uv*_Splat1_ST.z + dis.xy*0.09)*_Specular1;
				float3 colNormal2 = tex2D(_Normal2, i.uv*_Splat2_ST.z + dis.xy*0.09)*_Specular2;
				float3 colNormal3 = tex2D(_Normal3, i.uv*_Splat3_ST.z + dis.xy*0.09)*_Specular3;

				half4 col = tex2D(_MainTex, i.uv + dis.xy*0.09);
				float3 noise = tex2D(_Noise, i.uv*_TilingN2 + dis.xy)*_NoisePower;
				float3 grassPattern = lerp(grassPatternSplat0, grassPatternSplat1, splat_control.g);
				grassPattern = lerp(grassPattern, grassPatternSplat2, splat_control.b);
				grassPattern = lerp(grassPattern, grassPatternSplat3, splat_control.a);
				float GrassThinnessColor = lerp(_Splat0_ST.w, _Splat1_ST.w, splat_control.g);
				GrassThinnessColor = lerp(GrassThinnessColor, _Splat2_ST.w, splat_control.b);
				GrassThinnessColor = lerp(GrassThinnessColor, _Splat3_ST.w, splat_control.a)*_GrassThinness;
				half3 NoGrass = tex2D(_NoGrassTex, i.uv + dis.xy*0.05);
				NoGrass.r = saturate(NoGrass.r - splat_control.r);

				// Biplanar
				if (_UseBiplanar == 0)
				{
					//_BiPlanarSize = 1;
					//_BiPlanarStrength = 1;
				}
				else if (_UseBiplanar == 1)
				{
					float3 vec = mul(unity_ObjectToWorld, float4(i.normal, 0.0)).xyz;
					float threshold = smoothstep(_BiPlanarSize, _BiPlanarStrength, abs(dot(vec, float3(0, 1, 0))));
					NoGrass.r *= lerp(1, 0, threshold);
				}
				NoGrass.r = saturate(NoGrass.r + ripplesG);

				half alpha = step(1 - ((1 + grassPattern.x) * GrassThinnessColor)*((2 - i.color.r)*NoGrass.r*grassPattern.x)*saturate(ripples + 1)*saturate(ripples + 1), ((1 - i.color.r)*(ripples + 1))*(NoGrass.r*grassPattern.x)*GrassThinnessColor - dis.x * 5);
				alpha = lerp(alpha, alpha + (grassPattern.x*NoGrass.r*(1 - i.color.r))*_GrassThinnessIntersection, 1 - (NoGrass.r)*(ripples*NoGrass.r + 0.75));

				if (i.color.r >= 0.01)
				{
					if (alpha*(ripples3 + 1) - (i.color.r) < -0.02)discard;
				}
				SHADOW_CASTER_FRAGMENT(i)
				//return col; //For debugging
			}
			ENDCG
		}

		Pass{
		 Tags { "LightMode" = "ForwardAdd" }
		 // pass for additional light sources
Blend One One // Soft Additive

	  CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#pragma geometry geom

		#include "UnityCG.cginc"
		uniform float4 _LightColor0;
		uniform float4x4 unity_WorldToLight;
		uniform sampler2D _LightTexture0;

		  uniform float4 _SpecColor;
		  uniform float _Shininess;
		  sampler2D _MainTex;
		  sampler2D _NoGrassTex;
		  sampler2D _Noise;

		  uniform sampler2D _GlobalEffectRT;
		  uniform float3 _Position;
		  uniform float _OrthographicCamSize;
		  uniform sampler2D _Control;

		  uniform float _HasRT;
		  int _NumberOfStacks, _RTEffect, _MinimumNumberStacks, _UseBiplanar;
		  float4 _MainTex_ST;
		  sampler2D _Distortion;
		  sampler2D _GrassTex;
		  float _TilingN1;
		  float _WindForce, _TilingN2;
		  float4 _OffsetVector;
		  float _TilingN3, _BiPlanarStrength, _BiPlanarSize;
		  float _WindMovement, _OffsetValue, _FadeDistanceStart, _FadeDistanceEnd;
		  half _GrassThinness, _GrassThinnessIntersection, _GrassCut, _NoisePower, _GrassShading;
		  half4 _Specular0, _Specular1, _Specular2, _Specular3;
		  float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
		  sampler2D _Splat0, _Splat1, _Splat2, _Splat3, _Normal0, _Normal1, _Normal2, _Normal3;

		  struct appdata
		  {
			  float4 vertex : POSITION;
			  float2 uv : TEXCOORD0;
			  float3 normal : NORMAL;
			  float4 worldPos : TEXCOORD1;
			  float4 posLight : TEXCOORD2;
			  UNITY_VERTEX_INPUT_INSTANCE_ID
		  };

		  struct v2g {
			  float2 uv : TEXCOORD0;
			  float4 pos : SV_POSITION;
			 float3 normal : NORMAL;
			 float4 worldPos : TEXCOORD1;
			 float4 posLight : TEXCOORD2;
			 float4 objPos : TEXCOORD3;
			 UNITY_VERTEX_INPUT_INSTANCE_ID
		  };

		  struct g2f {
			  float2 uv : TEXCOORD0;
			  float4 pos : SV_POSITION;
			  float4 worldPos : TEXCOORD1;
			  float4 posLight : TEXCOORD2;
			  float1 color : TEXCOORD3;
			  float3 normal : TEXCOORD4;
			  UNITY_VERTEX_INPUT_INSTANCE_ID
			  UNITY_VERTEX_OUTPUT_STEREO
		  };

		  v2g vert(appdata v)
		  {
			  v2g o;
			  UNITY_SETUP_INSTANCE_ID(v);
			  UNITY_TRANSFER_INSTANCE_ID(v, o);

			  float4x4 modelMatrix = unity_ObjectToWorld;
			  float4x4 modelMatrixInverse = unity_WorldToObject;
			  o.objPos = v.vertex;
			  o.worldPos = mul(modelMatrix, v.vertex);
			  o.posLight = mul(unity_WorldToLight, o.worldPos);
			  o.normal = v.normal;
			  o.pos = UnityObjectToClipPos(v.vertex);
			  o.uv = TRANSFORM_TEX(v.uv, _MainTex);

			  return o;
		  }

#define UnityObjectToWorld(o) mul(unity_ObjectToWorld, float4(o.xyz,1.0))
		  [maxvertexcount(53)]
		  void geom(triangle v2g input[3], inout TriangleStream<g2f> tristream) {

			  g2f o;

			  _OffsetValue *= 0.01;

			  for (int i = 0; i < 3; i++)
			  {
				  UNITY_SETUP_INSTANCE_ID(input[i]);
				  UNITY_TRANSFER_INSTANCE_ID(input[i], o);
				  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				  o.uv = input[i].uv;
				  o.pos = input[i].pos;
				  o.color = float3(0 + _GrassCut, 0 + _GrassCut, 0 + _GrassCut);
				  o.normal = normalize(mul(float4(input[i].normal, 0.0), unity_WorldToObject).xyz);
				  o.worldPos = UnityObjectToWorld(input[i].objPos);
				  o.posLight = mul(unity_WorldToLight, input[i].worldPos);

				  tristream.Append(o);
			  }
			  float4 P;
			  float4 objSpace;
			  tristream.RestartStrip();

			  float dist = distance(_WorldSpaceCameraPos, UnityObjectToWorld((input[0].objPos / 3 + input[1].objPos / 3 + input[2].objPos / 3)));
			  if (dist > 0)
			  {
				  int NumStacks = lerp(_NumberOfStacks + 1, 0, (dist - _FadeDistanceStart)*(1 / max(_FadeDistanceEnd - _FadeDistanceStart, 0.0001)));//Clamp because people will start dividing by 0
				  _NumberOfStacks = min(clamp(NumStacks, clamp(_MinimumNumberStacks, 0, _NumberOfStacks), 17), _NumberOfStacks);
			  }

			  for (float i = 1; i <= _NumberOfStacks; i++)
			  {
				  float4 offsetNormal = _OffsetVector * i*0.01;
				  for (int ii = 0; ii < 3; ii++)
				  {
					  UNITY_SETUP_INSTANCE_ID(input[ii]);
					  UNITY_TRANSFER_INSTANCE_ID(input[ii], o);
					  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					  float thicknessModifier = 1;
					  float dist = distance(_WorldSpaceCameraPos, UnityObjectToWorld(input[ii].objPos));
					  if (dist > 0)
					  {
						  thicknessModifier = lerp(0.1*0.01, _OffsetValue, (dist - 1)*(1 / max(3 - 1, 0.0001)));//Clamp because people will start dividing by 0
						  thicknessModifier = clamp(thicknessModifier, 0.1*0.01, _OffsetValue);
					  }
					  float4 NewNormal = float4(normalize(input[ii].normal), 0);
					  objSpace = float4(input[ii].objPos + NewNormal * thicknessModifier*i + offsetNormal);
					  o.color = (i / (_NumberOfStacks - _GrassCut));
					  o.uv = input[ii].uv;
					  o.pos = UnityObjectToClipPos(objSpace);
					  o.worldPos = UnityObjectToWorld(objSpace);
					  o.normal = normalize(mul(float4(input[ii].normal, 0.0), unity_WorldToObject).xyz);
					  o.posLight = mul(unity_WorldToLight, input[ii].worldPos);

					  tristream.Append(o);
				  }
				  tristream.RestartStrip();
			  }
		  }

		  float4 frag(g2f i) : COLOR
		  {

			  float2 uv = i.worldPos.xz - _Position.xz;
				uv = uv / (_OrthographicCamSize * 2);
				uv += 0.5;

				float bRipple = 1;

				if (_HasRT)
				{
					if (_RTEffect == 1)
					{
						bRipple = 1 - clamp(tex2D(_GlobalEffectRT, uv).b * 5, 0, 2);
					}
				}

				float2 dis = tex2D(_Distortion, i.uv  *_TilingN3 + _Time.xx * 3 * _WindMovement);
				float displacementStrengh = 0.6* (((sin(_Time.y + dis * 5) + sin(_Time.y*0.5 + 1.051)) / 5.0) + 0.15*dis)*bRipple; //hmm math
				dis = dis * displacementStrengh*(i.color.r*1.3)*_WindForce*bRipple;

				float ripples = 0.25;
				float ripples2 = 0;
				float ripplesG = 0;
				float ripples3 = 0;

				if (_HasRT)
				{
					if (_RTEffect == 1)
					{
						// .b(lue) = Grass height / .r(ed) = Grass shadow / .g(reen) = Remove Grass
						ripples = (0.25 - tex2D(_GlobalEffectRT, uv + dis.xy*0.04).b);
						ripples2 = (tex2D(_GlobalEffectRT, uv + dis.xy*0.04).r);
						ripplesG = (0 - tex2D(_GlobalEffectRT, uv + dis.xy*0.04).g);
						ripples3 = (0 - ripples2)*ripples2;
					}
				}
				half4 splat_control = tex2D(_Control, i.uv + dis.xy*0.05);
				float3 grassPatternSplat0 = tex2D(_Splat0, i.uv*_TilingN1*_Splat0_ST.z + dis.xy);
				float3 grassPatternSplat1 = tex2D(_Splat1, i.uv*_TilingN1*_Splat1_ST.z + dis.xy);
				float3 grassPatternSplat2 = tex2D(_Splat2, i.uv*_TilingN1*_Splat2_ST.z + dis.xy);
				float3 grassPatternSplat3 = tex2D(_Splat3, i.uv*_TilingN1*_Splat3_ST.z + dis.xy);
				float3 colNormal0 = tex2D(_Normal0, i.uv*_Splat0_ST.z + dis.xy*0.09)*_Specular0;
				float3 colNormal1 = tex2D(_Normal1, i.uv*_Splat1_ST.z + dis.xy*0.09)*_Specular1;
				float3 colNormal2 = tex2D(_Normal2, i.uv*_Splat2_ST.z + dis.xy*0.09)*_Specular2;
				float3 colNormal3 = tex2D(_Normal3, i.uv*_Splat3_ST.z + dis.xy*0.09)*_Specular3;

				half4 col = tex2D(_MainTex, i.uv + dis.xy*0.09);
				float3 noise = tex2D(_Noise, i.uv*_TilingN2 + dis.xy)*_NoisePower;
				float3 grassPattern = lerp(grassPatternSplat0, grassPatternSplat1, splat_control.g);
				grassPattern = lerp(grassPattern, grassPatternSplat2, splat_control.b);
				grassPattern = lerp(grassPattern, grassPatternSplat3, splat_control.a);
				float GrassThinnessColor = lerp(_Splat0_ST.w, _Splat1_ST.w, splat_control.g);
				GrassThinnessColor = lerp(GrassThinnessColor, _Splat2_ST.w, splat_control.b);
				GrassThinnessColor = lerp(GrassThinnessColor, _Splat3_ST.w, splat_control.a)*_GrassThinness;
				half3 NoGrass = tex2D(_NoGrassTex, i.uv + dis.xy*0.05);
				NoGrass.r = saturate(NoGrass.r - splat_control.r);

				// Biplanar
				if (_UseBiplanar == 0)
				{
					//_BiPlanarSize = 1;
					//_BiPlanarStrength = 1;
				}
				else if (_UseBiplanar == 1)
				{
					float3 vec = mul(unity_ObjectToWorld, float4(i.normal, 0.0)).xyz;
					float threshold = smoothstep(_BiPlanarSize, _BiPlanarStrength, abs(dot(vec, float3(0, 1, 0))));
					NoGrass.r = lerp(1, 0, threshold);
				}
				NoGrass.r = saturate(NoGrass.r + ripplesG);

				half alpha = step(1 - ((1 + grassPattern.x) * GrassThinnessColor)*((2 - i.color.r)*NoGrass.r*grassPattern.x)*saturate(ripples + 1)*saturate(ripples + 1), ((1 - i.color.r)*(ripples + 1))*(NoGrass.r*grassPattern.x)*GrassThinnessColor - dis.x * 5);
				alpha = lerp(alpha, alpha + (grassPattern.x*NoGrass.r*(1 - i.color.r))*_GrassThinnessIntersection, 1 - (NoGrass.r)*(ripples + 0.75));

				if (i.color.r >= 0.01)
				{
					if (alpha*(ripples3 + 1) - (i.color.r) < -0.02)discard;
				}

			 float3 normalDirection = normalize(i.normal);
			 float3 lightDirection;
			 float attenuation;
			 float cookieAttenuation = 1.0;

			 if (0.0 == _WorldSpaceLightPos0.w) // directional light
			 {
				attenuation = 1.0; // no attenuation
				lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				cookieAttenuation = tex2D(_LightTexture0, i.posLight.xy).a;
			 }
			 else if (1.0 != unity_WorldToLight[3][3]) // spot light
			 {
				 attenuation = 1.0; // no attenuation
				 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				 cookieAttenuation = tex2D(_LightTexture0,i.posLight.xy / i.posLight.w + float2(0.5, 0.5)).a;
			 }
			 else // point light
			 {
				float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - i.worldPos.xyz;
				//float distance = length(vertexToLightSource);
				//attenuation = 1.0 / distance; // linear attenuation 
				lightDirection = normalize(vertexToLightSource);

				float3 lightCoord = mul(unity_WorldToLight, float4(i.worldPos.xyz, 1)).xyz;
				fixed atten = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL;
				half ndotl = saturate(dot(i.normal, lightDirection));
				attenuation = ndotl * atten;
			 }
			 float3 diffuseReflection = attenuation * _LightColor0.rgb  * max(0.0, dot(normalDirection, lightDirection));
			 float3 finalLightColor = cookieAttenuation * diffuseReflection*saturate(i.color.r+0.25);
			 return float4(finalLightColor, 1.0);
		  }

        ENDCG
	    }
		
		}// Fallback "VertexLit"
}
