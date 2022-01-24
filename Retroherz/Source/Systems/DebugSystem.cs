using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class DebugSystem : EntityDrawSystem
    {
        private readonly TiledMap _tiledMap;
        private readonly SpriteBatch _spriteBatch;
        private readonly OrthographicCamera _camera;
        private ComponentMapper<CircularColliderComponent> _circularColliderComponentMapper;
        private ComponentMapper<RectangularColliderComponent> _rectangularColliderComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public DebugSystem(GraphicsDevice graphicsDevice, OrthographicCamera camera, TiledMap tiledMap = null)
            : base(Aspect.One(
                typeof(CircularColliderComponent),
                typeof(RectangularColliderComponent),
                typeof(PhysicsComponent)))
        {
            _camera = camera;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _tiledMap = tiledMap;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _circularColliderComponentMapper = mapperService.GetMapper<CircularColliderComponent>();
            _rectangularColliderComponentMapper = mapperService.GetMapper<RectangularColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.Opaque,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _camera.GetViewMatrix());

                foreach(var entityId in ActiveEntities)
                {
                    var circularCollider = _circularColliderComponentMapper.Get(entityId);
                    var collider = _rectangularColliderComponentMapper.Get(entityId);
                    var physics = _physicsComponentMapper.Get(entityId);

                    // Draw players retangular collider
                    _spriteBatch.DrawRectangle(collider.Position, collider.Size, Color.Green);

                    // Draw player circular collider
                    _spriteBatch.DrawCircle(circularCollider.Position, circularCollider.Radius, 32, Color.Purple);

		            // Draw players velocity vector
                    _spriteBatch.DrawLine(
                        physics.Position + collider.Size / 2,
                        physics.Position + collider.Size / 2 + physics.Velocity.NormalizedCopy() * 20, Color.Yellow);
    
                    // Embellish the "in contact" rectangles in yellow
                    for (int i = 0; i < 4; i++)
                    {
                        if (collider.Contact[i] != null)
                            _spriteBatch.FillRectangle(collider.Contact[i].Position, collider.Contact[i].Size, Color.Yellow);
                        collider.Contact[i] = null;
                    }
                }
                
                if (_tiledMap != null)
                {
                    var tiles = _tiledMap.TileLayers[0].Tiles.Where(x => !x.IsBlank);
                    foreach(var tile in tiles)
                    {
                        //System.Console.WriteLine("{0},{1},{2},{3}", tile.X, tile.Y, _tiledMap.TileWidth, _tiledMap.TileHeight);

                        var rectangleF = new RectangleF(tile.X * _tiledMap.TileWidth, tile.Y * _tiledMap.TileHeight, _tiledMap.TileWidth, _tiledMap.TileHeight);
                        _spriteBatch.DrawRectangle(rectangleF, Color.Red);
                    }
                }

            _spriteBatch.End();
        }
    }
}