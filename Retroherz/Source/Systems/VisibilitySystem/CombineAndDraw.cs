using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Retroherz.Math;

namespace Retroherz.Systems;
// Courtesy of Roy Triesscheijn - based on and modified by Vidar "Voidar" Ermesjø
// https://roy-t.nl/2014/02/27/2d-lighting-and-shadows-preview.html
// http://roy-t.nl/files/ShadowsIn2D.zip

public partial class VisibilitySystem
{
	/// <summary>
	/// Combines what is in the color map with the lighting in the light map.
	/// </summary>
	internal void CombineAndDraw()
	{
		_graphics.SetRenderTarget(null);
		_graphics.Clear(Color.Black);

		_graphics.BlendState = BlendState.Opaque;
		// Samplers states are set by the shader itself            
		_graphics.DepthStencilState = DepthStencilState.Default;
		_graphics.RasterizerState = RasterizerState.CullCounterClockwise;

		_effect.Combine.Parameters["colorMap"].SetValue(_renderTarget.ColorMap);
		_effect.Combine.Parameters["lightMap"].SetValue(_renderTarget.LightMap);

		_effect.Combine.Techniques[0].Passes[0].Apply();
		quad.Render(_graphics, -Vector2.One, Vector2.One);
	}
}