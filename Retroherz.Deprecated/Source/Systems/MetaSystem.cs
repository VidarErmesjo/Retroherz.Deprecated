using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
	public class MetaSystem : EntityProcessingSystem
	{
		public ComponentMapper<MetaComponent> _metaComponentMapper;

		public MetaSystem()
			: base(Aspect.All(typeof(MetaComponent)))
		{
		}

		public override void Initialize(IComponentMapperService mapperService)
		{
			_metaComponentMapper = mapperService.GetMapper<MetaComponent>();
		}

		public override void Process(GameTime gameTime, int entityId)
		{
			var meta = _metaComponentMapper.Get(entityId);
		}
	}
}