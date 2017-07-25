texture read;
float2 gridSize;
float2 gridOffset;
float scale;

sampler2D read_sampler : register(s1) = sampler_state
{
	Texture = <read>;
	Filter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float2 uv = coords + gridOffset.xy / gridSize.xy;
	return float4(scale * tex2D(read_sampler, uv).xyz, 1.0);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
