texture velocity;
texture vorticity;
float2 gridSize;
float gridScale;
float timestep;
float epsilon;
float2 curl;

sampler2D vorticity_sampler : register(s1) = sampler_state
{
	Texture = <vorticity>;
	Filter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

sampler2D velocity_sampler : register(s2) = sampler_state
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

	float vl = tex2D(vorticity_sampler, uv - xOffset).x;
	float vr = tex2D(vorticity_sampler, uv + xOffset).x;
	float vb = tex2D(vorticity_sampler, uv - yOffset).x;
	float vt = tex2D(vorticity_sampler, uv + yOffset).x;
	float vc = tex2D(vorticity_sampler, uv).x;

	float scale = 0.5 / gridScale;
	float2 force = scale * float2(abs(vt) - abs(vb), abs(vr) - abs(vl));
	float lengthSquared = max(epsilon, dot(force, force));
	force *= rsqrt(lengthSquared) * curl * vc;
	force.y *= -1.0;

	float2 velc = tex2D(velocity_sampler, uv).xy;
	return float4(velc + (timestep * force), 0.0, 1.0);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
