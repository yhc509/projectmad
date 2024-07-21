#if !defined(LOOKING_THROUGH_WATER_INCLUDED)
#define LOOKING_THROUGH_WATER_INCLUDED

//_CameraDepthTexture ... globaly available depth buffer 
//_WaterBackground ... GrubPass
sampler2D _CameraDepthTexture, _WaterBackground; 
float4 _CameraDepthTexture_TexelSize; //bug and different implementation, unity wiki stuff

//exponential underwater fog properties
float3 _WaterFogColor;
float _WaterFogDensity;

float _RefractionStrength;


//remove the thin line from other grabed materials
//fron unity wiki	
float2 AlignWithGrabTexel (float2 uv) {
	//BUGS & DIFFERENT IMPLEMENTATION
	//on some platforms we have the coordinates upside down for some reason
	//unity wiki
	#if UNITY_UV_STARTS_AT_TOP
		if (_CameraDepthTexture_TexelSize.y < 0) {
			uv.y = 1 - uv.y;
		}
	#endif

	return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) * abs(_CameraDepthTexture_TexelSize.xy);
}

float3 ColorBelowWater (float4 screenPos, float3 tangentSpaceNormal) {
	//fake refractions, use the rangentspace normal, si it will WIGGLE
	//this is synchronized with the motion of the surface
	//for a flat surface, bouth are 0, higher wave rippel -> greater offsett
	float2 uvOffset = tangentSpaceNormal.xy * _RefractionStrength;
	//we have to change the .y offset, since we get an image, which is NOT symetrical
	uvOffset.y *= _CameraDepthTexture_TexelSize.z * abs(_CameraDepthTexture_TexelSize.y);

	//... screenPos == clipspace -> we have to divide .xy by .w to get the final depth texture coordinates
	float2 uv = AlignWithGrabTexel((screenPos.xy + uvOffset) / screenPos.w);
	
	//getting the underwater depth:
	//// sample background depth and convert the raw value to linear depth (depth relative to the screen, not the water surface)
	float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	//// distance between water and screen, we need liner depth again
	float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
	//// now just find the inderwater depth
	float depthDifference = backgroundDepth - surfaceDepth;
	
	uvOffset *= saturate(depthDifference);
	
	//check weather we actually hit the forground of the water, in which case DO NOT change the uvOffset 
	uv = AlignWithGrabTexel((screenPos.xy + uvOffset) / screenPos.w);
	//using correct depth for the fog
	backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	depthDifference = backgroundDepth - surfaceDepth;
	
	//to actually not just render black/whit, we can now use the _WaterBackground from the grubPass just before drawing
	float3 backgroundColor = tex2D(_WaterBackground, uv).rgb;
	//just exponential fog as we get deeper under water
	float fogFactor = exp2(-_WaterFogDensity * depthDifference);
	//interpolate the color using the fog factor
	return lerp(_WaterFogColor, backgroundColor, fogFactor);
}

#endif