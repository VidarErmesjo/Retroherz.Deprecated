#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

const float3 black = float3(0.0f, 0.0f, 0.0f);

float2 lightSource; // Light position in 2D world space
float3 lightColor;  // Color of the light
float  lightRadius; // Radius of the light

struct VertexShaderInput
{
  float3 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
  float4 Position : POSITION0;
  float2 WorldPos : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
  VertexShaderOutput output;
  output.Position = float4(input.Position, 1);
  output.WorldPos = input.TexCoord; // Vertex position in 2D world space
  return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{   
  // Compute the relative position of the pixel
  // and the distance towards the light
  float2 position = input.WorldPos - lightSource;     
  float dist = sqrt(dot(position, position));

  // Mix between black and the light color based on the distance
  // and the power of the light
  float3 mix = lerp(lightColor, black, saturate(dist / lightRadius));
  return float4(mix, 1.0f);
}

technique Technique1
{
  pass Pass1
  {
    VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
    PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
  }
}
