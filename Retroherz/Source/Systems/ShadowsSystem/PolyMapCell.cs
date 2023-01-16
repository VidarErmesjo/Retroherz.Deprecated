namespace Retroherz.Systems;

internal struct PolyMapCell
{
	public (int Id, bool Exists) Northern;
	public (int Id, bool Exists) Southern;
	public (int Id, bool Exists) Eastern;
	public (int Id, bool Exists) Western;
	public bool Exists;

	//public PolyMapCell(bool exists) => Exists = exists;
}