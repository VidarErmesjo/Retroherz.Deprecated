using System;
using Microsoft.Xna.Framework;
using Retroherz.Math;

namespace Retroherz.Systems;
// Courtesy of Roy Triesscheijn - based on and modified by Vidar "Voidar" Ermesjø
// https://roy-t.nl/2014/02/27/2d-lighting-and-shadows-preview.html
// http://roy-t.nl/files/ShadowsIn2D.zip

public partial class VisibilitySystem
{
	/// <summary>
	/// Represents the end-point of a segment.
	/// </summary>
	internal class EndPoint : IEquatable<EndPoint>
	{
		/// <summary>
		/// Position of the segment.
		/// </summary>
		internal Vector2 Position { get; set; }

		/// <summary>
		/// If this end-point is a begin or end end-point
		/// of a segment (each segment has only one begin and one end end-point.
		/// </summary>
		internal bool Begin { get; set; }

		/// <summary>
		/// The segment this end-point belongs to.
		/// </summary>
		internal Segment Segment { get; set; }

		/// <summary>
		/// The angle of the end-point relative to the location of the visibility test.
		/// </summary>
		internal double Angle { get; set; }

		internal EndPoint()
		{
			Position = Vector2.Zero;
			Begin = false;
			Segment = null;
			Angle = 0;
		}

		/// <summary>
		/// Returns true if the end-point equals the other end-point.
		/// </summary>
		public bool Equals(EndPoint other) => (
			(Position == other.Position) &&
			(Begin == other.Begin) &&
			(Angle == other.Angle)
		);

		// Overrides
		/// <summary>
		/// Returns true if the end-point equals the object.
		/// </summary>
		public override bool Equals(object obj) => (
			(obj is EndPoint other) &&
			(Position, Begin, Angle).Equals((other.Position, other.Begin, other.Angle))
			// We do not care about the segment beeing the same 
			// since that would create a circular reference - Roy Triesscheijn
		);

		/// <summary>
		/// Returns the hash code of the end-point.
		/// </summary>
		public override int GetHashCode() => (Position, Begin, Angle).GetHashCode();

		/// <summary>
		/// Returns the end-point information.
		/// </summary>
		public override string ToString() => "{" + $"Position:{Position} Angle:{Angle} Segment:{Segment}" + "}";
	}
}