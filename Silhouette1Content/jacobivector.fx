texture x;
texture b;
float2 gridSize;
float alpha;
float beta;

sampler2D x_sampler : register(s1) = sampler_state
{
	Texture = <x>;
	Filter = POINT;
};

sampler2D b_sampler : register(s2) = sampler_state
{
	Texture = <b>;
	Filter = POINT;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float2 uv = coords;
	
	float2 xOffset = float2(1.0 / gridSize.x, 0.0);
	float2 yOffset = float2(0.0, 1.0 / gridSize.y);
	
	float2 xl = tex2D(x_sampler, uv - xOffset).xy;
	float2 xr = tex2D(x_sampler, uv + xOffset).xy;
	float2 xb = tex2D(x_sampler, uv - yOffset).xy;
	float2 xt = tex2D(x_sampler, uv + yOffset).xy;
	
	float2 bc = tex2D(b_sampler, uv).xy;

	return float4((xl + xr + xb + xt + alpha * bc) / beta, 0.0, 1.0);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
