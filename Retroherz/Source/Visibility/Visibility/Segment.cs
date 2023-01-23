using System;

namespace Retroherz.Visibility.Deprecated;
// Courtesy of Roy Triesscheijn - based on and modified by Vidar "Voidar" Ermesjø
// https://roy-t.nl/2014/02/27/2d-lighting-and-shadows-preview.html
// http://roy-t.nl/files/ShadowsIn2D.zip

/// <summary>
/// Represents an occluding line segment in the visibility mesh.
/// </summary>
internal class Segment : IEquatable<Segment>
{
	/// <summary>
	/// First end-point of the segment.
	/// </summary>
	internal EndPoint P1 { get; set; }

	/// <summary>
	/// Second end-point of the segment.
	/// </summary>
	internal EndPoint P2 { get; set; }

	internal Segment()
	{
		P1 = null;
		P2 = null;            
	}

	/// <summary>
	/// Returns true if the segment equals the other segment.
	/// </summary>
	public bool Equals(Segment other) => (
		(P1.Position == other.P1.Position) &&
		(P2.Position == other.P2.Position)
	);

	// Overrides
	/// <summary>
	/// Returns true if the segment equals the object.
	/// </summary>
	public override bool Equals(object obj) => (
		(obj is Segment other) &&
		(P1.Position, P2.Position).Equals((other.P1.Position, other.P2.Position))
	);

	/// <summary>
	/// Returns the hash code of the segnment.
	/// </summary>
	public override int GetHashCode() => (P1.Position, P2.Position).GetHashCode();

	/// <summary>
	/// Returns the segment information.
	/// </summary>
	public override string ToString() => "{" + $"P1:{P1.Position} P2:{P2.Position}" + "}";
}