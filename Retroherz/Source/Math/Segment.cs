using System;
namespace Retroherz.Math;

/// <summary>
///	Represents a line segment from start to end.
/// </summary>
public class Segment : IEquatable<Segment>
{
	public Vector Start;
	public Vector End;

	public Segment(Vector start = default(Vector), Vector end = default(Vector))
	{
		Start = start;
		End = start;
	}

	public static bool operator ==(Segment p, Segment q) => p.Equals(q);
	public static bool operator !=(Segment p, Segment q) => !(p == q);

	// Implementations
    /// <summary>
    /// Returns true if the segment equals the other segment.
    /// </summary>
	public bool Equals(Segment other) => (
		(Start == other.Start) &&
		(End == other.End)
	);

	// Overrides
    /// <summary>
    /// Returns true if the segment equals the object.
    /// </summary>
	public override bool Equals(object obj) => (
		(obj is Segment other) &&
		(other.Start, other.End).Equals((Start, End))
	);

    /// <summary>
    /// Returns the HashCode of the segment.
    /// </summary>
	public override int GetHashCode() => (Start, End).GetHashCode();

    /// <summary>
    /// Returns the segment information.
    /// </summary>
	public override string ToString() => "{" + $"Start:{Start} End:{End}" + "}";
}