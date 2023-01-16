using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
	// Move to PlayerSystem??
	public class SelectSystem : EntityUpdateSystem
	{
		private readonly InputManager _inputManager;
		private readonly Bag<int> _focused;
		private readonly Bag<int> _selected;

		private ComponentMapper<ColliderComponent> _colliderComponentMapper;
		private ComponentMapper<MetaComponent> _metaComponentMapper;
		private ComponentMapper<PlayerComponent> _playerComponentMapper;
		private ComponentMapper<TransformComponent> _transformComponentMapper;

		public SelectSystem(InputManager inputManager)
			: base(Aspect.All(typeof(ColliderComponent), typeof(MetaComponent), typeof(TransformComponent)))
		{
			_inputManager = inputManager;

			_focused = new();
			_selected = new();
		}

		public override void Initialize(IComponentMapperService mapperService)
		{
			_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
			_metaComponentMapper = mapperService.GetMapper<MetaComponent>();
			_playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
			_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
		}

		public override void Update(GameTime gameTime)
		{
			var deltaTime = gameTime.GetElapsedSeconds();

			foreach (int entityId in ActiveEntities.AsReadOnlySpan())
			{
				(ColliderComponent collider, MetaComponent meta, PlayerComponent player, TransformComponent transform) entity = new(
					_colliderComponentMapper.Get(entityId),
					_metaComponentMapper.Get(entityId),
					_playerComponentMapper.Get(entityId),
					_transformComponentMapper.Get(entityId)
				);

				if (_inputManager.State == InputManagerState.MouseDragStart)
					_selected.Clear();

				// Assign to player when available
				if (entity.player is not null)
				{
					entity.player.Focused = _focused;
					entity.player.Selected = _selected;
				}

				// Get focused and selected entities
				var predictive = Predictive.BoundingRectangle(entity.collider, entity.transform, deltaTime);
				if (predictive.Intersects(_inputManager.Selection))
				{
					if(!_focused.Contains(entityId))
						_focused.Add(entityId);

					if (_inputManager.State == InputManagerState.MouseDragEnd)
						foreach (var item in _focused)
							_selected.Add(item);
				}
				else
				{
					_focused.Remove(entityId);
				}

			}
		}
	}
}