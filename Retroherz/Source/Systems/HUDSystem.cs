using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;

namespace Retroherz.Systems
{
	public class HUDSystem : EntityDrawSystem
	{
		private readonly OrthographicCamera _camera;
		private readonly InputManager _inputManager;
		private readonly SpriteBatch _spriteBatch;

		private ComponentMapper<ColliderComponent> _colliderComponenetMapper;
		private ComponentMapper<PlayerComponent> _playerComponentMapper;
		private ComponentMapper<TransformComponent> _transformComponentMapper;

		public HUDSystem(OrthographicCamera camera, InputManager inputManager, SpriteBatch spriteBatch)
			: base(Aspect.All(typeof(ColliderComponent), typeof(PlayerComponent), typeof(TransformComponent)))
		{
			_camera = camera;
			_inputManager = inputManager;
			_spriteBatch = spriteBatch;
		}

		public override void Initialize(IComponentMapperService mapperService)
		{
			_colliderComponenetMapper = mapperService.GetMapper<ColliderComponent>();
			_playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
			_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
		}

		public override void Draw(GameTime gameTime)
		{
			var deltaTime = gameTime.GetElapsedSeconds();

			_spriteBatch.Begin(
				sortMode: SpriteSortMode.FrontToBack,
				blendState: BlendState.Additive,
				samplerState: SamplerState.PointClamp,
				transformMatrix: _camera.GetViewMatrix());

				// Draw selection area
				if (_inputManager.State != InputManagerState.Idle)
					_spriteBatch.DrawRectangle(_inputManager.Selection, Color.White);

				foreach (int playerId in ActiveEntities.AsReadOnlySpan())
				{
					var player = _playerComponentMapper.Get(playerId) ?? throw new($"HUDSystem():{playerId} == null");

					foreach (var focusedId in player.Focused)
					{
						// Check if entity is still alive
						if (GetEntity(focusedId) is null)
						{
							player.Focused.Remove(focusedId);
							continue;
						}

						(ColliderComponent collider, TransformComponent transform) focused = new(
							_colliderComponenetMapper.Get(focusedId),
							_transformComponentMapper.Get(focusedId)
						);

						_spriteBatch.DrawRectangle(new(focused.transform.Position, focused.collider.Size), Color.White);
					}

					foreach(var selectedId in player.Selected)
					{
						// Check if entity is still alive
						if (GetEntity(selectedId) is null)
						{
							player.Focused.Remove(selectedId);
							continue;
						}

						(ColliderComponent collider, TransformComponent transform) selected = new(
							_colliderComponenetMapper.Get(selectedId),
							_transformComponentMapper.Get(selectedId)
						);

						_spriteBatch.FillRectangle(new(selected.transform.Position, selected.collider.Size), Color.Green);
					}
				}

			_spriteBatch.End();
		}
	}
}