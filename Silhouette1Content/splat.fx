texture read;
float2 gridSize;
float3 color;
float2 center;
float radius;

sampler2D read_sampler : register(s1) = sampler_state
{
	Texture = <read>;
	Filter = POINT;
};

float gauss(float2 p, float r)
{
	return exp(-dot(p, p) / r);
}

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float2 uv = coords;
	float3 base = tex2D(read_sampler, uv).xyz;
	float2 coord = center.xy - uv;
	float3 splat = color * gauss(coord, gridSize.x * radius);
	return float4(base + splat, 1.0);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
