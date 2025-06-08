#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

struct Attributes
{
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
};

struct Varyings
{
	float4 positionCS_SS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
};

Varyings UnlitPassVertex(Attributes input)
{
	Varyings output;
	float3 positionWS = TransformObjectToWorld(input.positionOS);
	output.positionCS_SS = TransformWorldToHClip(positionWS);
	output.baseUV.xy = input.baseUV.xy;
	return output;
}

float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
	float4 textureColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.baseUV);
	// subtract a small offset for opacity precision errors
	clip(textureColor.a - 0.001);

	return textureColor;
}

#endif
