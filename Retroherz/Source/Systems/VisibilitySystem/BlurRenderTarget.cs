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
	/// Blurs the target render target.
	/// </summary>   
	internal void BlurRenderTarget(RenderTarget2D target, float strength)
	{
		Vector2 renderTargetSize = new(target.Width, target.Height);

		_effect.Blur.Parameters["renderTargetSize"].SetValue(renderTargetSize);            
		_effect.Blur.Parameters["blur"].SetValue(strength);

		// Pass one
		_graphics.SetRenderTarget(_renderTarget.BlurMap);
		_graphics.Clear(Color.Black);

		_effect.Blur.Parameters["InputTexture"].SetValue(target);
		
		_effect.Blur.CurrentTechnique = _effect.Blur.Techniques["BlurHorizontally"];
		_effect.Blur.CurrentTechnique.Passes[0].Apply();
		quad.Render(_graphics, -Vector2.One, Vector2.One);

		// Pass two
		_graphics.SetRenderTarget(target);
		_graphics.Clear(Color.Black);
		
		_effect.Blur.Parameters["InputTexture"].SetValue(_renderTarget.BlurMap);

		_effect.Blur.CurrentTechnique = _effect.Blur.Techniques["BlurVertically"];
		_effect.Blur.CurrentTechnique.Passes[0].Apply();
		quad.Render(_graphics, -Vector2.One, Vector2.One);
	}
}