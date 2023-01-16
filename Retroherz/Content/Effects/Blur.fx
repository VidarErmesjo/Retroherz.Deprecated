#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 renderTargetSize;
float blur = 2.5f;

texture InputTexture;
sampler inputSampler = sampler_state
{
  Texture = <InputTexture>;
  MipFilter = Point;
  MinFilter = Point;
  MagFilter = Point;
  AddressU = Clamp;
  AddressV = Clamp;
};

static const int kernelSize = 13;
static const float2 offsetAndWeight[kernelSize] =
{
  { -6, 0.002216 },
  { -5, 0.008764 },
  { -4, 0.026995 },
  { -3, 0.064759 },
  { -2, 0.120985 },
  { -1, 0.176033 },
  { 0, 0.199471 },
  { 1, 0.176033 },
  { 2, 0.120985 },
  { 3, 0.064759 },
  { 4, 0.026995 },
  { 5, 0.008764 },
  { 6, 0.002216 },
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


VertexShaderOutput FullScreenVS(VertexShaderInput input)
{
  VertexShaderOutput output;
  output.Position = float4(input.Position, 1);
  output.TexCoord = input.TexCoord;

  return output;
}


float4 BlurHorizontallyPS(float2 TexCoord  : TEXCOORD0) : COLOR0
{  
  float3 sum = float3(0, 0, 0);
  for (int i = 0; i < kernelSize; i++)
  {        
    sum += tex2D(inputSampler,      
                  TexCoord + offsetAndWeight[i].x *
                  blur / renderTargetSize.x *
                  float2(1, 0)).rgb *
                 offsetAndWeight[i].y;
  }  

  return float4(sum, 1.0f);
}

float4 BlurVerticallyPS(float2 TexCoord  : TEXCOORD0) : COLOR0
{  
  float3 sum = float3(0, 0, 0);  

  for (int i = 0; i < kernelSize; i++)
  {
    sum += tex2D(inputSampler,
                  TexCoord + offsetAndWeight[i].x *
                  blur / renderTargetSize.y *
                  float2(0, 1)).rgb *
                offsetAndWeight[i].y;
  }

  return float4(sum, 1.0f); 
}

technique BlurHorizontally
{
  pass P0
  {
    VertexShader = compile VS_SHADERMODEL FullScreenVS();
    PixelShader = compile PS_SHADERMODEL BlurHorizontallyPS();
  }
}

technique BlurVertically
{
  pass P0
  {
    VertexShader = compile VS_SHADERMODEL FullScreenVS();
    PixelShader = compile PS_SHADERMODEL BlurVerticallyPS();
  }
}