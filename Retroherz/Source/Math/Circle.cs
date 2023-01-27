using System;
namespace Retroherz.Math;

public struct Circle
{
	public Vector Center = default(Vector);
	public float Radius = default(float);

	public Circle(Vector center, float radius)
	{
		Center = center;
		Radius = radius;
	}

	/// <summary>
	///	Determines whether a line, defined by the two vectors u and v, intersects this circle.
	/// </summary>
	public bool Intersects(Vector u, Vector v)
	{
		Vector a, b;
		return Intersects(u.X, u.Y, v.X, v.Y, out a, out b);
	}

	/// <summary>
	///	Determines whether a line, defined by the two vectors u and v, intersects this circle.
	/// </summary>
	public bool Intersects(Vector u, Vector v, out Vector a, out Vector b) => this.Intersects(
		u.X,
		u.Y,
		v.X,
		v.Y,
		out a,
		out b
	);

	/// <summary>
	///	Determines whether a line, defined by two points, intersects this circle.
	/// </summary>
	public bool Intersects(float x1, float y1, float x2, float y2, out Vector u, out Vector v)
	// https://www.desmos.com/calculator/0kadyeba6y
	{
		float a = y2 - y1;
		float b = x2 - x1;
		float c1 = this.Center.X;
		float c2 = this.Center.Y;
		float A = (a * a + b * b);	// length
		float B = -c1 * b - c2 * a + x1 * b + y1 * a; 
		float C = (c1 * c1 + c2 * c2) + (x1 * x1 + y1 * y1) - (2 * c1 * x1) - (2 * c2 * y1) - (this.Radius * this.Radius);
		float D = (B * B) - (4 * A * C);

		// Parametric line equation
		Func<float, (float, float)> f = (float t) => (x1 + b * t, y1 + a * t);

		if (D > 0)
		{
			// p2 = (x2, y2) => j1 => u
			// p1 = (x1, y1) => j2 => v
			float j1, j2;
			(float X, float Y) j;
			float dSquared = MathF.Sqrt(D);

			j1 = (-B + dSquared) / (2 * B);
			j2 = (-B - dSquared) / (2 * B);

			j = f(j2);
			v = C < 0 ? (x1, y1) : j;

			j = f(j1);
			C = (c1 * c1 + c2 * c2) + (x2 * x2 + y2 * y2) - (2 * c1 * x2) - (2 * c2 * y2) - (this.Radius * this.Radius);
			u = C < 0 ? (x2, y2) : j;

			return true;
		}

		u = v = Vector.Zero;

		return false;
	}
}