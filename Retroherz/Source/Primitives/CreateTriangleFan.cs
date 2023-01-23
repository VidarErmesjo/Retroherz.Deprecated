using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using Retroherz.Math;

namespace Retroherz;

public static partial class PrimitiveExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Primitive CreateTriangleFan(this Primitive primitive, ReadOnlySpan<Vector> points, Vector origin)
	{
		if (points.Length < 1)
			return primitive;

		Span<short> indices = stackalloc short[points.Length * 3];
		Span<VertexPositionTexture> vertices = stackalloc VertexPositionTexture[points.Length + 1];

		// Add a vertex for the center of the mesh
		vertices[0] = new VertexPositionTexture(
			new Vector3(origin.X, origin.Y, 0),
			origin
		);

		// Add all the other encounter points as vertices
		// storing their world position as UV coordinates.
		for (int i = 0; i < points.Length; i++)
			vertices.Slice(1)[i] = new VertexPositionTexture(
				new Vector3(points[i].X, points[i].Y, 0),
				points[i]	// Convert to UV?
			);

		// Compute the indices to form a "triangle-fan" (counter clockwise / right-hand-rule).
		int k = 2;
		for (int j = 2;  j < vertices.Length; j++)
		{
			indices[k - 2] = 0;
			indices[k - 1] = ((short)(j));
			indices[k] = ((short)(j - 1)); 

			k += 3;
		}

		// Fan will have one open edge, so draw last point of fan to first and last point.
		indices[indices.Length - 3] = 0;
		indices[indices.Length - 2] = 1;
		indices[indices.Length - 1] = ((short)(vertices.Length - 1));

		return new Primitive(vertices, indices);
	}
}