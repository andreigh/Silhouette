texture p;
texture w;
float2 gridSize;
float gridScale;

sampler2D p_sampler : register(s1) = sampler_state
{
	Texture = <p>;
	Filter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

sampler2D w_sampler : register(s2) = sampler_state
{
	Texture = <w>;
	Filter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float2 uv = coords;

	float2 xOffset = float2(1.0 / gridSize.x, 0.0);
	float2 yOffset = float2(0.0, 1.0 / gridSize.y);

	float pl = tex2D(p_sampler, uv - xOffset).x;
	float pr = tex2D(p_sampler, uv + xOffset).x;
	float pb = tex2D(p_sampler, uv - yOffset).x;
	float pt = tex2D(p_sampler, uv + yOffset).x;

	float scale = 0.5 / gridScale;
	float2 gradient = scale * float2(pr - pl, pt - pb);

	float2 wc = tex2D(w_sampler, uv).xy;

	return float4(wc - gradient, 0.0, 1.0);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
