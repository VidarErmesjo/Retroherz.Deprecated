using System;
namespace Retroherz.Components
{
	public enum MetaComponentType
	{
		Unknow,
		Player,
		NPC,
		Enemy,
		Wall
	}

	public class MetaComponent
	{
		public readonly int Id;
		public readonly string DisplayName;
		public readonly MetaComponentType Type;

		public MetaComponent(int id, MetaComponentType type = MetaComponentType.Unknow, string displayName = null)
		{
			Id = id;
			DisplayName = displayName ?? type.ToString();
			Type = type;
		}

		public override string ToString() => "{" + $"Id:{Id} DisplayName:{DisplayName}" + "}";

		~MetaComponent() {}
	}
}