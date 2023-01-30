using Retroherz.Math;

namespace Retroherz.Visibility;

public struct PolyMap// : IComparable<PolyMap>
{
	public Vector Start = default(Vector);
	public Vector End = default(Vector);

	public PolyMap(Vector start, Vector end)
	{
		Start = start;
		End = end;
	}
}