using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Retroherz.Math;

namespace Retroherz.Systems;
// Courtesy of Roy Triesscheijn - based on and modified by Vidar "Voidar" Ermesjø
// https://roy-t.nl/2014/02/27/2d-lighting-and-shadows-preview.html
// http://roy-t.nl/files/ShadowsIn2D.zip

public partial class ShadowsSystem
{
	/// <summary>
	/// Internal component to render a quad without touching render states.
	/// </summary>
	internal class Quad
	{
		private VertexPositionTexture[] vertices;
		private short[] indexBuffer;

		internal Quad()
		{
			vertices = new VertexPositionTexture[]
						{
							new VertexPositionTexture(
								new Vector3(0, 0, 0),
								new Vector2(1, 1)),
							new VertexPositionTexture(
								new Vector3(0, 0, 0),
								new Vector2(0, 1)),
							new VertexPositionTexture(
								new Vector3(0, 0, 0),
								new Vector2(0, 0)),
							new VertexPositionTexture(
								new Vector3(0, 0, 0),
								new Vector2(1, 0))
						};

			indexBuffer = new short[] { 0, 1, 2, 2, 3, 0 };
		}

		/// <summary>
		/// Renders the four vertices for a quad directly without touching render states,
		/// setting shaders, etc...
		/// </summary>        
		internal void Render(GraphicsDevice device, Vector2 bottomLeft, Vector2 topRight)
		{
			vertices[0].Position.X = topRight.X;
			vertices[0].Position.Y = bottomLeft.Y;

			vertices[1].Position.X = bottomLeft.X;
			vertices[1].Position.Y = bottomLeft.Y;

			vertices[2].Position.X = bottomLeft.X;
			vertices[2].Position.Y = topRight.Y;

			vertices[3].Position.X = topRight.X;
			vertices[3].Position.Y = topRight.Y;

			device.DrawUserIndexedPrimitives<VertexPositionTexture>
			(
				PrimitiveType.TriangleList,
				vertices,
				0,
				4,
				indexBuffer,
				0,
				2
			);
		}
	}
}