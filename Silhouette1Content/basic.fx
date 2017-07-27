texture ScreenTexture;
float4x4 MatrixTransform;

sampler ScreenS : register(s1) = sampler_state
{
	Texture = <ScreenTexture>;
	Filter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
// 	float4 color = float4(0.5,0.5,0.5,0.0) + float4(0.5,0.5,0.5,1.0) * tex2D(ScreenS, coords);
	float4 color = tex2D(ScreenS, coords);
	return color;
}

void SpriteVertexShader(inout float4 color : COLOR0,
	inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	position = mul(position, MatrixTransform);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
		VertexShader = compile vs_3_0 SpriteVertexShader();
	}
}
