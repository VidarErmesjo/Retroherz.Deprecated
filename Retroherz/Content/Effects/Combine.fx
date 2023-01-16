#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture colorMap;
texture lightMap;

sampler colorSampler = sampler_state
{
  Texture = (colorMap);
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  Mipfilter = LINEAR;
};
sampler lightSampler = sampler_state
{
  Texture = (lightMap);
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  Mipfilter = LINEAR;
};

struct VertexShaderInput
{
  float3 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
  VertexShaderOutput output;
  output.Position = float4(input.Position, 1);
  output.TexCoord = input.TexCoord;
  return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
  float3 diffuseColor = tex2D(colorSampler, input.TexCoord).rgb;
  float3 light = tex2D(lightSampler, input.TexCoord).rgb;   
  return float4((diffuseColor * light), 1);  
}

technique Technique1
{
  pass Pass1
  {
    VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
    PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
  }
}
