using System;
using System.Buffers;
using Microsoft.Xna.Framework.Graphics;

namespace Retroherz;

public struct Primitive
{
	private ReadOnlyMemory<VertexPositionTexture> _vertices;
	private ReadOnlyMemory<short> _indices;

	public ReadOnlySpan<VertexPositionTexture> Vertices => _vertices.Span;
	public ReadOnlySpan<short> Indices => _indices.Span;

	// TODO
	public PrimitiveType PrimitiveType { get; set; }
	public int PrimitiveCount { get; set; }

	public Primitive(ReadOnlySpan<VertexPositionTexture> vertices, ReadOnlySpan<short> indices)
	{
		_vertices = new ReadOnlyMemory<VertexPositionTexture>(vertices.ToArray());
		_indices = new ReadOnlyMemory<short>(indices.ToArray());
	}
}

public static partial class PrimitiveExtensions
{
	public static void Draw(this Primitive primitive, GraphicsDevice graphics)
	{
		if (primitive.Vertices.Length < 3)
			return;

		graphics.DrawUserIndexedPrimitives<VertexPositionTexture>
		(
			PrimitiveType.TriangleList,
			primitive.Vertices.ToArray(),
			0,
			primitive.Vertices.Length,
			primitive.Indices.ToArray(),
			0,
			primitive.Indices.Length / 3
		);
	}
}