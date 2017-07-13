texture ScreenTexture;

sampler ScreenS : register(s1) = sampler_state
{
	Texture = <ScreenTexture>;
	Filter = POINT;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float4 color = tex2D(ScreenS, coords-float2(0.001,0.002));
	return color;//float4(1, 0, 0, 1);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}