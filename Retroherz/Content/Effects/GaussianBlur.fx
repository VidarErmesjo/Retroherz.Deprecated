#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// The shader texture will be the input texture we are blurring.
/*Texture2D shaderTexture;
SamplerState SampleType;

struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
    float2 texCoord1 : TEXCOORD1;
    float2 texCoord2 : TEXCOORD2;
    float2 texCoord3 : TEXCOORD3;
    float2 texCoord4 : TEXCOORD4;
    float2 texCoord5 : TEXCOORD5;
    float2 texCoord6 : TEXCOORD6;
    float2 texCoord7 : TEXCOORD7;
    float2 texCoord8 : TEXCOORD8;
    float2 texCoord9 : TEXCOORD9;
};

float4 HorizontalBlurPixelShader(PixelInputType input) : SV_TARGET
{
    float weight0, weight1, weight2, weight3, weight4;
    float normalization;
    float4 color;
//As discussed in the algorithm we determine the color of this pixel by averaging the eight total neighbors and the center pixel. However the value we use for each neighbor is also modified by a weight. The weights we use for this tutorial give the closest neighbors a greater effect on the average than the more distant neighbors.

    // Create the weights that each neighbor pixel will contribute to the blur.
    weight0 = 1.0f;
    weight1 = 0.9f;
    weight2 = 0.55f;
    weight3 = 0.18f;
    weight4 = 0.1f;
//With the weight values set we will then normalize them to create a smoother transition in the blur.

    // Create a normalized value to average the weights out a bit.
    normalization = (weight0 + 2.0f * (weight1 + weight2 + weight3 + weight4));

    // Normalize the weights.
    weight0 = weight0 / normalization;
    weight1 = weight1 / normalization;
    weight2 = weight2 / normalization;
    weight3 = weight3 / normalization;
    weight4 = weight4 / normalization;
//To create the blurred pixel we first set the color to black and then we add the center pixel and the eight neighbors to the final color based on the weight of each.

    // Initialize the color to black.
    color = float4(0.0f, 0.0f, 0.0f, 0.0f);

    // Add the nine horizontal pixels to the color by the specific weight of each.
    color += shaderTexture.Sample(SampleType, input.texCoord1) * weight4;
    color += shaderTexture.Sample(SampleType, input.texCoord2) * weight3;
    color += shaderTexture.Sample(SampleType, input.texCoord3) * weight2;
    color += shaderTexture.Sample(SampleType, input.texCoord4) * weight1;
    color += shaderTexture.Sample(SampleType, input.texCoord5) * weight0;
    color += shaderTexture.Sample(SampleType, input.texCoord6) * weight1;
    color += shaderTexture.Sample(SampleType, input.texCoord7) * weight2;
    color += shaderTexture.Sample(SampleType, input.texCoord8) * weight3;
    color += shaderTexture.Sample(SampleType, input.texCoord9) * weight4;
//Finally we set the alpha value.

    // Set the alpha channel to one.
    color.a = 1.0f;

    return color;
}

float4 VerticalBlurPixelShader(PixelInputType input) : SV_TARGET
{
    float weight0, weight1, weight2, weight3, weight4;
    float normalization;
    float4 color;
//We use the same weighting system/values that the horizontal blur shader also used.

    // Create the weights that each neighbor pixel will contribute to the blur.
    weight0 = 1.0f;
    weight1 = 0.9f;
    weight2 = 0.55f;
    weight3 = 0.18f;
    weight4 = 0.1f;

    // Create a normalized value to average the weights out a bit.
    normalization = (weight0 + 2.0f * (weight1 + weight2 + weight3 + weight4));

    // Normalize the weights.
    weight0 = weight0 / normalization;
    weight1 = weight1 / normalization;
    weight2 = weight2 / normalization;
    weight3 = weight3 / normalization;
    weight4 = weight4 / normalization;

    // Initialize the color to black.
    color = float4(0.0f, 0.0f, 0.0f, 0.0f);

    // Add the nine vertical pixels to the color by the specific weight of each.
    color += shaderTexture.Sample(SampleType, input.texCoord1) * weight4;
    color += shaderTexture.Sample(SampleType, input.texCoord2) * weight3;
    color += shaderTexture.Sample(SampleType, input.texCoord3) * weight2;
    color += shaderTexture.Sample(SampleType, input.texCoord4) * weight1;
    color += shaderTexture.Sample(SampleType, input.texCoord5) * weight0;
    color += shaderTexture.Sample(SampleType, input.texCoord6) * weight1;
    color += shaderTexture.Sample(SampleType, input.texCoord7) * weight2;
    color += shaderTexture.Sample(SampleType, input.texCoord8) * weight3;
    color += shaderTexture.Sample(SampleType, input.texCoord9) * weight4;

    // Set the alpha channel to one.
    color.a = 1.0f;

    return color;
}*/

#define RADIUS 7
#define KERNEL_SIZE (RADIUS * 2 + 1)

float weights[KERNEL_SIZE];
float2 offsets[KERNEL_SIZE];

texture screenTexture;

sampler2D colorMap = sampler_state
{
    Texture = <screenTexture>;
    MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
};

 
//------------------------ PIXEL SHADER ----------------------------------------
// This pixel shader will simply look up the color of the texture at the
// requested point
float4 GaussianBlur(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
    
    for(int i = 0; i < KERNEL_SIZE; ++i)
        color += tex2D(colorMap, texCoord + offsets[i]) * weights[i];
        
    return color;
}
 
//-------------------------- TECHNIQUES ----------------------------------------
// This technique is pretty simple - only one pass, and only a pixel shader
technique GaussianBlur
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL GaussianBlur();
    }
}