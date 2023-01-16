namespace Retroherz.Systems;
public partial class ShadowsSystem
{
	internal struct PolyMapCell
	{
		public (int Id, bool Exists) North;
		public (int Id, bool Exists) South;
		public (int Id, bool Exists) East;
		public (int Id, bool Exists) West;
		public required bool Exists;
	}
}