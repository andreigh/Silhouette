texture velocity;
float2 gridSize;
float gridScale;

sampler2D velocity_sampler : register(s1) = sampler_state
{
	Texture = <velocity>;
	Filter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float2 uv = coords;

	float2 xOffset = float2(1.0 / gridSize.x, 0.0);
	float2 yOffset = float2(0.0, 1.0 / gridSize.y);

	float vl = tex2D(velocity_sampler, uv - xOffset).y;
	float vr = tex2D(velocity_sampler, uv + xOffset).y;
	float vb = tex2D(velocity_sampler, uv - yOffset).x;
	float vt = tex2D(velocity_sampler, uv + yOffset).x;

	float scale = 0.5 / gridScale;
	return float4(scale * (vr - vl - vt + vb), 0.0, 0.0, 1.0);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
