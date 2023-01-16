using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;

namespace Retroherz
{
	public static class EntityManager
	{
		private static readonly Bag<int> _entities = new();
		private static World _world;

		public static bool Contains(int element) => _entities.Contains(element);
		public static int EntityCount => _entities.Count;
		public static World World => _world;

		public static void Initialize(in World world)
		{
			_world = world;
		}

		public static Entity CreateEntity()
		{
			var entity = _world.CreateEntity();

			if (!_entities.Contains(entity.Id))
				_entities.Add(entity.Id);

			return entity;
		}

		public static void DestroyEntity(int entityId)
		{
			_world.DestroyEntity(entityId);
			_entities.Remove(entityId);
		}
		
	}
}