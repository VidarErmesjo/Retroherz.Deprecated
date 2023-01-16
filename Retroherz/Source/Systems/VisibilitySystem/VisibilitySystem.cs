using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using Retroherz.Components;

namespace Retroherz.Systems;

public partial class VisibilitySystem : EntityDrawSystem
{
	private readonly OrthographicCamera _camera;
	private readonly (Effect Blur, Effect Combine, Effect Light) _effect;
	private readonly GraphicsDevice _graphics;
	private readonly Bag<int> _obstacles = new();
	private readonly (RenderTarget2D ColorMap, RenderTarget2D LightMap, RenderTarget2D BlurMap) _renderTarget;
	private readonly SpriteBatch _spriteBatch;

	private ComponentMapper<ColliderComponent> _colliderComponentMapper;
	private ComponentMapper<PointLightComponent> _pointLightComponentMapper;
	private ComponentMapper<TransformComponent> _transformComponentMapper;

	internal float deltaTime;
	internal readonly Quad quad = new();

	public VisibilitySystem(
		OrthographicCamera camera,
		(Effect Blur, Effect Combine, Effect Light) effect,
		GraphicsDevice graphics,
		(RenderTarget2D ColorMap, RenderTarget2D LightMap, RenderTarget2D BlurMap) renderTarget,
		SpriteBatch spriteBatch
	)
		: base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)))
	{
		_camera = camera;
		_effect = effect;
		_graphics = graphics;
		_renderTarget = renderTarget;	
		_spriteBatch = spriteBatch;
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
		_transformComponentMapper = mapperService.GetMapper<TransformComponent>();
		_pointLightComponentMapper = mapperService.GetMapper<PointLightComponent>();
	}

	public override void Draw(GameTime gameTime)
	{

		// Draw the colors
		DrawColorMap();
		
		// Draw the lights
		DrawLightMap(0.0f);                     
		
		// Blur the shadows
		//BlurRenderTarget(_renderTarget.LightMap, 2.5f);

		// Combine
		CombineAndDraw();

	}
}