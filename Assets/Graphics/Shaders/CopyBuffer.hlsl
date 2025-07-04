#ifndef COPY_BUFFER_SHADERS_INCLUDED
#define COPY_BUFFER_SHADERS_INCLUDED

TEXTURE2D(_SourceTexture);

struct Varyings
{
	float4 positionCS_SS : SV_POSITION;
	float2 screenUV : VAR_SCREEN_UV;
};

Varyings DefaultPassVertex(uint vertexID : SV_VertexID)
{
	// define single screen space triangle to blit into
	Varyings output;
	output.positionCS_SS = float4(
		vertexID <= 1 ? -1.0 : 3.0,
		vertexID == 1 ? 3.0 : -1.0,
		0.0, 1.0
	);
	output.screenUV = float2(
		vertexID <= 1 ? 0.0 : 2.0,
		vertexID == 1 ? 2.0 : 0.0
	);

	// mirror render vertically if upside down (Direct3D). See https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
	if (_ProjectionParams.x < 0.0)
	{
		output.screenUV.y = 1.0 - output.screenUV.y;
	}
	return output;
}

float4 CopyPassFragment(Varyings input) : SV_TARGET
{
	return SAMPLE_TEXTURE2D_LOD(
		_SourceTexture, sampler_linear_clamp, input.screenUV, 0);
}

float CopyDepthPassFragment(Varyings input) : SV_DEPTH
{
	return SAMPLE_DEPTH_TEXTURE_LOD(
		_SourceTexture, sampler_point_clamp, input.screenUV, 0);
}

#endif
