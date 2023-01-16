using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using Retroherz.Components;

namespace Retroherz.Systems;
// Courtesy of Roy Triesscheijn - based on and modified by Vidar "Voidar" Ermesjø
// https://roy-t.nl/2014/02/27/2d-lighting-and-shadows-preview.html
// http://roy-t.nl/files/ShadowsIn2D.zip

public partial class VisibilitySystem
{
	/// <summary>
	///	Draw all normal objects.
	/// </summary>
	internal void DrawColorMap()
	{
		_graphics.SetRenderTarget(_renderTarget.ColorMap);
		_graphics.Clear(Color.CornflowerBlue);

		_spriteBatch.Begin(
			sortMode: SpriteSortMode.BackToFront,
			blendState: BlendState.AlphaBlend,
			samplerState: SamplerState.PointClamp,
			depthStencilState: DepthStencilState.Default,
			rasterizerState: RasterizerState.CullCounterClockwise,
			effect: null,
			transformMatrix: _camera.GetViewMatrix()
		);

		// Clear obstacles
		_obstacles.Clear();
		foreach (int entityId in ActiveEntities.AsReadOnlySpan())
		{	
			(ColliderComponent collider, TransformComponent transform) entity = new(
				_colliderComponentMapper.Get(entityId),
				_transformComponentMapper.Get(entityId)
			);

			// Do not process what can not be seen ????
			if (!_camera.BoundingRectangle.Intersects(Predictive.BoundingRectangle(
				entity.collider,
				entity.transform,
				deltaTime
			)))
				continue;

			//entity.sprite?.Draw(_spriteBatch);
			_obstacles.Add(entityId);
		}

		/*foreach (Visual v in blocks)
		{
			Vector2 origin = new Vector2(v.Texture.Width / 2.0f, v.Texture.Height / 2.0f);

			_spriteBatch.Draw(v.Texture, v.Pose.Position, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0);
		}*/

		_spriteBatch.End();
	}
}