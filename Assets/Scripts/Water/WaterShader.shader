Shader "Custom/DistortionFlow" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		// a Curl noise map
		[NoScaleOffset] _FlowMap ("Flow (RG, A noise)", 2D) = "black" {}
		[NoScaleOffset] _DerivHeightMap ("Deriv (AG) Height (B)", 2D) = "black" {}
		_Tiling ("Tiling", Float) = 1
		_Speed ("Speed", Float) = 1
		_FlowStrength ("Flow Strength", Float) = 1
		_HeightScale ("Height Scale, Constant", Float) = 0.25
		_HeightScaleModulated ("Height Scale, Modulated", Float) = 0.75
		_WaterFogColor ("Water Fog Color", Color) = (0, 0, 0, 0)
		_WaterFogDensity ("Water Fog Density", Range(0, 2)) = 0.1
		_RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.25
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200
		
		//to adjust the color of the background, we have to retrieve it
		//unity now ads an extra step in the rendering pipeline, just before drawing water
		//now all water surfaces will use the same texture: "_waterbg"
		GrabPass { "_WaterBackground" }

		CGPROGRAM
		#pragma surface surf Standard alpha finalcolor:ResetAlpha
		#pragma target 3.0

		#include "LookingThroughWater.cginc"

		sampler2D _MainTex, _FlowMap, _DerivHeightMap;
		float _Tiling, _Speed, _FlowStrength;
		float _HeightScale, _HeightScaleModulated;
		
		
		struct Input {
			float2 uv_MainTex;
			float4 screenPos; //sampling the depth texture
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float3 FlowUVW(float2 uv, float2 flowVector, float tiling, float time, bool flowB)
		{
			//used so that we can sample the same texture twice
			//just with a different offset 
			float phaseOffset = flowB ? 0.5 : 0;
			float progress = frac(time + phaseOffset); //sawtooth pattern
			float3 uvw;
			//.xy for sampling uv
			//.z for fading in/out of our looping sawtooth pattern (hiding the pattern)

			uvw.xy = uv - flowVector * progress;

			//we need a tiling value seperately, since we DONT want to tile the flowmap
			uvw.xy *= tiling;
			uvw.xy += phaseOffset;
			//phase sliding
			uvw.xy += time - progress;

			//triangle wave, which matches the sawtooth pattern
			uvw.z = 1 - abs(1 - 2 * progress);
			return uvw;
		}

		float3 UnpackDerivativeHeight (float4 textureData) {
			float3 dh = textureData.agb; //this is not a texture, this is a rgb map
			//so extract the correct channels
			//height derivatives in x, y
			//x derivative in A channel
			//y der stored in G channel
			//original height map in B channel, which we can use to scale heights of waves
			dh.xy = dh.xy * 2 - 1; // the texture can point in any direction, normalize it
			return dh;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 flow = tex2D(_FlowMap, IN.uv_MainTex).rgb;
			//just use x,y for the flow map
			//alpha is the perlin noise
			flow.xy = flow.xy * 2 - 1;
			flow *= _FlowStrength;//just another way of changing the flow speedof the flowmap

			//first channel of the map is the perlin nopise
			//used for fading distortion, so that we DO NOT fade in/out of the 
			//sawtooth pattern everywhereat once
			float noise = tex2D(_FlowMap, IN.uv_MainTex).a;
			float time = _Time.y * _Speed + noise;

			//using 2 textures to hide the "black" fade in andout pattern
			//using 2 triangle waves, which always sum up to 1
			//so we are never completely black
			//we are not actually using 2 textures, we are still using the same one though
			float3 uvwA = FlowUVW( IN.uv_MainTex, flow.xy, _Tiling, time, false);
			float3 uvwB = FlowUVW(IN.uv_MainTex, flow.xy, _Tiling, time, true);

			//now scale the wave height of the heightscale AND the wavespeed!
			float finalHeightScale = flow.z * _HeightScaleModulated + _HeightScale;

			float3 dhA = UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwA.xy)) * (uvwA.z * finalHeightScale);
			float3 dhB = UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwB.xy)) * (uvwB.z * finalHeightScale);
			o.Normal = normalize(float3(-(dhA.xy + dhB.xy), 1));

			fixed4 texA = tex2D(_MainTex, uvwA.xy) * uvwA.z;
			fixed4 texB = tex2D(_MainTex, uvwB.xy) * uvwB.z;

			fixed4 c = (texA + texB) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			o.Alpha = c.a; //restore the original alpha, thisaffects the surface lighting
			
			//we can NOT add the fog to the Alpha -> Albedo is affected by lighting
			//we want to add the fog to the surface lighting
			o.Emission = ColorBelowWater(IN.screenPos, o.Normal) * (1 - c.a);
		}
		
		//we dont want to blend twice with the backgorund
		//reset the alpha once we finish
		//gets called in #pragma
 		void ResetAlpha (Input IN, SurfaceOutputStandard o, inout fixed4 color) {
			color.a = 1;
		}
		
		ENDCG
	}
}