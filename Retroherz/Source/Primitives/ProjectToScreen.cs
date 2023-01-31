using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
namespace Retroherz;

public static partial class PrimitiveExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Primitive ProjectToScreen(this Primitive primitive, in OrthographicCamera camera)
	{
		Span<VertexPositionTexture> vertices = stackalloc VertexPositionTexture[primitive.Vertices.Length];
		Span<short> indices = stackalloc short[primitive.Indices.Length];

		primitive.Vertices.CopyTo(vertices);
		primitive.Indices.CopyTo(indices);

		for (int i = 0; i < vertices.Length; i++)
			vertices[i].Position = Vector3.Transform(
				vertices[i].Position,
				Matrix.CreateOrthographicOffCenter(
					camera.BoundingRectangle.Left,
					camera.BoundingRectangle.Right,
					camera.BoundingRectangle.Bottom,
					camera.BoundingRectangle.Top,
					0,
					1
				)
			);

		return new Primitive(vertices, indices);
	}
}