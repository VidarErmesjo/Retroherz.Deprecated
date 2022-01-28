using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public struct ColliderObject
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Size;
        public Vector2 PenetrationVector;

        public ColliderObject(
            Vector2 position = default(Vector2),
            Vector2 velocity = default(Vector2),
            Vector2 size = default(Vector2))
        {
            Position = position;
            Velocity = velocity;
            Size = size;
            PenetrationVector = Vector2.Zero;
        }
    }

    public class ColliderSystem : EntityProcessingSystem
    {
        private readonly CollisionComponent _collisionComponent;
        private readonly TiledMap _tiledMap;
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        private readonly List<ColliderComponent> _colliders;

        public ColliderSystem(TiledMap tiledMap)
            : base(Aspect.All(typeof(ColliderComponent), typeof(PhysicsComponent)))
        {
            _collisionComponent = new CollisionComponent(new RectangleF(
                0,
                0,
                tiledMap.Width * tiledMap.TileWidth,
                tiledMap.Height * tiledMap.TileHeight));
            _tiledMap = tiledMap;
            _colliders = new List<ColliderComponent>();
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();

            var tiles = _tiledMap.TileLayers[0].Tiles.Where(x => !x.IsBlank);

            foreach (var tile in tiles)
            {
                var rectangle = new RectangleF
                    (tile.X * _tiledMap.TileWidth,
                    tile.Y * _tiledMap.TileHeight,
                    _tiledMap.TileWidth,
                    _tiledMap.TileHeight);

                var collider = new ColliderComponent(
                    rectangle,
                    ColliderComponent.ColliderComponentType.Static);

                _collisionComponent.Insert(collider);

                _colliders.Add(collider);
            }
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var deltaTime = gameTime.GetElapsedSeconds();
            var collider = _colliderComponentMapper.Get(entityId);
            var physics = _physicsComponentMapper.Get(entityId);


            collider.Bounds.Position += physics.Velocity * deltaTime; //physics.Position;
            //_colliderComponentMapper.Put(entityId, new ColliderComponent(collider.Bounds, collider.Type));
            _collisionComponent.Insert(collider);

            _collisionComponent.Update(gameTime);

            //if (float.IsNaN(collider.PenetrationNormal.X) || float.IsNaN(collider.PenetrationNormal.Y))
            System.Console.WriteLine(collider.PenetrationNormal);


            // ClampVelocity
            ClampVelocity(ref physics, ref collider);

            physics.Position = collider.Bounds.Position;
            /*_physicsComponentMapper.Put(entityId, new PhysicsComponent(
                collider.Bounds.Position,
                physics.Velocity,
                physics.Direction));*/
        }

        private static void ClampVelocity(ref PhysicsComponent physics, ref ColliderComponent collider)
        {
            //collider.PenetrationNormal.SetX(float.IsNaN(collider.PenetrationNormal.X) ? 0f : collider.PenetrationNormal.X);
            //collider.PenetrationNormal.SetY(float.IsNaN(collider.PenetrationNormal.Y) ? 0f : collider.PenetrationNormal.Y);
            
            physics.Velocity = collider.PenetrationNormal.X > 0 ? new Vector2(
                MathHelper.Clamp(physics.Velocity.X, float.MinValue, 0),
                physics.Velocity.Y) : physics.Velocity;

            physics.Velocity = collider.PenetrationNormal.X < 0 ? new Vector2(
                MathHelper.Clamp(physics.Velocity.X, 0, float.MaxValue),
                physics.Velocity.Y) : physics.Velocity;

            physics.Velocity = collider.PenetrationNormal.Y > 0 ? new Vector2(
                physics.Velocity.X,
                MathHelper.Clamp(physics.Velocity.Y, float.MinValue, 0)) : physics.Velocity;

            physics.Velocity = collider.PenetrationNormal.Y < 0 ? new Vector2(
                physics.Velocity.X,
                MathHelper.Clamp(physics.Velocity.Y, 0, float.MaxValue)) : physics.Velocity;

            collider.PenetrationNormal = Vector2.Zero;


            /*var x = collider.PenetrationNormal.X > 0 ? MathHelper.Clamp(physics.Velocity.X, float.MinValue, 0) : physics.Velocity.X;

            x = collider.PenetrationNormal.X < 0 ? MathHelper.Clamp(physics.Velocity.X, 0, float.MaxValue) : x;

            var y = collider.PenetrationNormal.Y > 0 ? MathHelper.Clamp(physics.Velocity.Y, float.MinValue, 0) : physics.Velocity.Y;

            y = collider.PenetrationNormal.Y < 0 ? MathHelper.Clamp(physics.Velocity.Y, 0, float.MaxValue) : y;

            physics.Velocity = new Vector2(x, y);*/      
        }

        ~ColliderSystem() {}
    }
}