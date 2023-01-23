using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using Retroherz.Components;
using Retroherz.Managers;
using Retroherz.Math;

namespace Retroherz.Systems;

public class PlayerSystem : EntityProcessingSystem
{
	private readonly AssetsManager _assetsManager;
	private readonly OrthographicCamera _camera;
	private readonly InputManager _inputManager;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<PlayerComponent> _playerComponentMapper;
	private ComponentMapper<SpriteComponent> _spriteComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	//private Entity _player;

	public PlayerSystem(AssetsManager assetsManager, OrthographicCamera camera, InputManager inputManager)
		: base(Aspect
			.All(
				typeof(ColliderComponent),
				typeof(PlayerComponent),
				typeof(SpriteComponent),
				typeof(TransformComponent)))
	{
		_assetsManager = assetsManager;
		_camera = camera;
		_inputManager = inputManager;
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
		_playerComponentMapper = mapperService.GetMapper<PlayerComponent>();
		_spriteComponentMapper = mapperService.GetMapper<SpriteComponent>();
		_transformComponentMapper = mapperService.GetMapper<TransformComponent>();

		// CreatePlayer()
		/*Vector2 position = Vector2.One * 16 * 7;
		position.Y += 9;
		Vector2 size = new(32, 32);

		_player = CreateEntity();
		_player.Attach(new MetaComponent(id: _player.Id, type: MetaComponentType.Player));
		_player.Attach(new PlayerComponent());
		_player.Attach(new SpriteComponent(asepriteDocument: _assetsManager.Sprite("Shitsprite")));
		_player.Attach(new TransformComponent(position: position));
		_player.Attach(new ColliderComponent(size: size, type: ColliderComponentType.Dynamic));
		_player.Attach(new RayComponent(radius: 64));*/
	}

	public override void Process(GameTime gameTime, int entityId)
	{

		var deltaTime = gameTime.GetElapsedSeconds();

		var mouseState = _inputManager.CurrentMouseState;// Mouse.GetState();
		var keyboardState = Keyboard.GetState();

		var collider = _colliderComponentMapper.Get(entityId);           
		var player = _playerComponentMapper.Get(entityId);
		var sprite = _spriteComponentMapper.Get(entityId);
		var transform = _transformComponentMapper.Get(entityId);

		Vector2 direction = Vector2.Zero;
		if (keyboardState.GetPressedKeyCount() > 0)
		{
			if (keyboardState.IsKeyDown(Keys.Up))
				direction += -Vector2.UnitY;
			if (keyboardState.IsKeyDown(Keys.Down))
				direction += Vector2.UnitY;
			if (keyboardState.IsKeyDown(Keys.Left))
				direction += -Vector2.UnitX;
			if (keyboardState.IsKeyDown(Keys.Right))
				direction += Vector2.UnitX;

			if (direction != Vector2.Zero)
				direction.Normalize();

			// Important! Must check if is NaN
			if(direction.IsNaN()) direction = Vector2.Zero;

			collider.Velocity += direction * player.MaxSpeed * deltaTime * 2;

			/* if (keyboardState.IsKeyDown(Keys.Space))
				collider.Velocity = new Vector2(collider.Velocity.X, -100f);*/

			sprite.Play("Walk");
		}
		else if (mouseState.LeftButton == ButtonState.Pressed)
		{
			// Accelerate
			direction = Vector2.Normalize(_camera.ScreenToWorld(
				new(mouseState.X, mouseState.Y)) - (transform.Position - collider.HalfExtents));

			collider.Velocity += direction * player.MaxSpeed * deltaTime;
			
			/*collider.Velocity = new Vector2((
				MathHelper.Clamp(collider.Velocity.X, -player.MaxSpeed, player.MaxSpeed)),
				MathHelper.Clamp(collider.Velocity.Y, -player.MaxSpeed, player.MaxSpeed));*/

			sprite.Play("Walk");
		}
		else
			sprite.Play("Idle");

		// Exp.
		//collider.Size += Vector2.One * deltaTime;
		//collider.Size = new(32, 32);
		// Update position if size has changed
		/*if (collider.DeltaOrigin != Vector2.Zero)
			transform.Position += -collider.DeltaOrigin;*/
	}
}