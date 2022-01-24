#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//------------------------------ TEXTURE PROPERTIES ----------------------------
// This is the texture that SpriteBatch will try to set before drawing
texture screenTexture;
int width;// = 3840;
int height;// = 2160;
float pixelation;
 
// Our sampler for the texture, which is just going to be pretty simple
sampler textureSampler = sampler_state
{
    Texture = <screenTexture>;
};
 
//------------------------ PIXEL SHADER ----------------------------------------
// This pixel shader will simply look up the color of the texture at the
// requested point
float4 Pixelator(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord *= float2(width, height);

	float2 output;
	output.x = round(texCoord.x / pixelation) * pixelation;
	output.y = round(texCoord.y / pixelation) * pixelation;
	output /= float2(width, height);

    float4 color = tex2D(textureSampler, output);    
    return color;
}
 
//-------------------------- TECHNIQUES ----------------------------------------
// This technique is pretty simple - only one pass, and only a pixel shader
technique Pixelator
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL Pixelator();
    }
}