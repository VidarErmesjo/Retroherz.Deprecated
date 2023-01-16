using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using Retroherz.Components;
using Retroherz.Math;

namespace Retroherz.Systems;
// Courtesy of Roy Triesscheijn - based on and modified by Vidar "Voidar" Ermesjø
// https://roy-t.nl/2014/02/27/2d-lighting-and-shadows-preview.html
// http://roy-t.nl/files/ShadowsIn2D.zip

public partial class VisibilitySystem
{
	/// <summary>
	/// Draws everything that emits light to a seperate render target.
	/// </summary>
	internal void DrawLightMap(float ambientLightStrength = 1)
	{
		_graphics.SetRenderTarget(_renderTarget.LightMap);
		_graphics.Clear(Color.White * ambientLightStrength);

		// Draw normal object that emit light
		_spriteBatch.Begin(
			sortMode: SpriteSortMode.BackToFront,
			blendState: BlendState.AlphaBlend,
			samplerState: SamplerState.PointClamp,
			depthStencilState: DepthStencilState.Default,
			rasterizerState: RasterizerState.CullCounterClockwise
		);

		/*foreach(Visual v in blocks)
		{
			if(v.Glow != null)
			{
				Vector2 origin = new Vector2(v.Glow.Width / 2.0f, v.Glow.Height / 2.0f);
				spriteBatch.Draw(v.Glow, v.Pose.Position, null, Color.White, v.Pose.Rotation, origin, v.Pose.Scale, SpriteEffects.None, 0);
			}
		}*/

		_spriteBatch.End();

		_graphics.BlendState = BlendState.Additive;
		// Samplers states are set by the shader itself            
		_graphics.DepthStencilState = DepthStencilState.None;
		_graphics.RasterizerState = RasterizerState.CullNone;

		/*if (Mouse.GetState().RightButton == ButtonState.Pressed)
		{
			lights[0].Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
		}*/
		
		/*foreach (PointLight l in lights)
		{                
			l.Render(GraphicsDevice, blocks);
		}*/

		foreach (int sourceId in ActiveEntities.AsReadOnlySpan())
		{
			(ColliderComponent collider, PointLightComponent light, TransformComponent transform) source =(
				_colliderComponentMapper.Get(sourceId),
				_pointLightComponentMapper.Get(sourceId),
				_transformComponentMapper.Get(sourceId)
			);

			// Skip if ...
			if (source.light is null)
				continue;

			// "Render"
            if (source.light.Power > 0)
            {
				// Compute the visibility mesh
				VisibilityComputer visibility = new(
					source.transform.Position + source.collider.Origin,
					source.light.Radius
				);


				foreach (int obstacleId in _obstacles.AsReadOnlySpan())
				{
					// Do not process self
					if (obstacleId == sourceId)
						continue;

					(ColliderComponent collider, TransformComponent transform) obstacle =(
						_colliderComponentMapper.Get(sourceId),
						_transformComponentMapper.Get(sourceId)
					);

                    //float width = obstacle.collider.Size.X * v.Texture.Width;
                    visibility.AddSquareOccluder(
						obstacle.transform.Position + obstacle.collider.Origin,
						((float)obstacle.collider.Size.X),
						0
					);
				}

                var encounters = visibility.Compute();

                // Generate a triangle list from the encounter points
                VertexPositionTexture[] vertices;
                short[] indices;

                TriangleListFromEncounters(encounters, source.transform.Position + source.collider.Origin, out vertices, out indices);

                // Project the vertices to the screen
                ProjectVertices(
					vertices,
					_camera.BoundingRectangle.Width,
					_camera.BoundingRectangle.Height
					//_graphics.PresentationParameters.BackBufferWidth,
					//_graphics.PresentationParameters.BackBufferHeight
				);

                // Apply the effect

                _effect.Light.Parameters["lightSource"].SetValue(source.transform.Position + source.collider.Size);
                _effect.Light.Parameters["lightColor"].SetValue(source.light.Color.ToVector3() * source.light.Power);
                _effect.Light.Parameters["lightRadius"].SetValue(source.light.Radius);
                _effect.Light.Techniques[0].Passes[0].Apply();

                // Draw the light on screen, using the triangle fan from the computed
                // visibility mesh so that the light only influences the area that can be 
                // "seen" from the light's position.
                _graphics.DrawUserIndexedPrimitives<VertexPositionTexture>
                (
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    indices,
                    0,
                    indices.Length / 3
                );
            }
		}

	}

	private void ProjectVertices(VertexPositionTexture[] vertices, float screenWidth, float screenHeight)
	{
		float halfScreenWidth = screenWidth / 2.0f;
		float halfScreenHeight = screenHeight / 2.0f;

		// Computes the screen coordinates from the world coordinates
		// TODO: this should be done using a proper camera system
		// using a view-projection matrix in the shader
		for (int i = 0; i < vertices.Length; i++)
		{
			VertexPositionTexture current = vertices[i];

			current.Position.X = (current.Position.X / halfScreenWidth) - _camera.Position.X - 1.0f;
			current.Position.Y = ((screenHeight - current.Position.Y) / halfScreenHeight) - _camera.Position.Y - 1.0f;

			/*Vector2 worldToScreen = _camera.WorldToScreen(current.Position.X, current.Position.Y);
			current.Position.X = worldToScreen.X;
			current.Position.Y = worldToScreen.Y;*/

			vertices[i] = current;
		}       
	}      

	private void TriangleListFromEncounters(List<Vector2> encounters,
		Vector2 position,
		out VertexPositionTexture[] vertexArray,
		out short[] indexArray)
	{
		List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();

		// Add a vertex for the center of the mesh
		vertices.Add(new VertexPositionTexture(new Vector3(position.X, position.Y, 0),
			position));

		// Add all the other encounter points as vertices
		// storing their world position as UV coordinates
		foreach (Vector2 vertex in encounters)
		{
			vertices.Add(new VertexPositionTexture(new Vector3(vertex.X, vertex.Y, 0),
				vertex));
		}

		// Compute the indices to form triangles
		List<short> indices = new List<short>();
		for (int i = 0; i < encounters.Count; i += 2)
		{
			indices.Add(0);
			indices.Add((short)(i + 2));
			indices.Add((short)(i + 1));                
		}

		vertexArray = vertices.ToArray<VertexPositionTexture>();
		indexArray = indices.ToArray<short>();
	}
}