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
    public class ColliderSystem : EntityProcessingSystem
    {
        private readonly CollisionComponent _collisionComponent;
        private readonly TiledMap _tiledMap;
        private readonly List<RectangularColliderComponent> _obstacles;
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public ColliderSystem(TiledMap tiledMap)
            : base(Aspect.All(typeof(ColliderComponent), typeof(PhysicsComponent)))
        {
            _collisionComponent = new CollisionComponent(new RectangleF(
                0,
                0,
                tiledMap.Width * tiledMap.TileWidth,
                tiledMap.Height * tiledMap.TileHeight));
            _obstacles = new List<RectangularColliderComponent>();
            _tiledMap = tiledMap;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();

            var tiles = _tiledMap.TileLayers[0].Tiles.Where(x => !x.IsBlank);

            foreach (var tile in tiles)
            {
                var collider = new ColliderComponent(new RectangleF
                    (tile.X * _tiledMap.TileWidth,
                    tile.Y * _tiledMap.TileHeight,
                    _tiledMap.TileWidth,
                    _tiledMap.TileHeight),
                    ColliderComponent.ColliderComponentType.Static);

                _collisionComponent.Insert(collider);

                _obstacles.Add(new RectangularColliderComponent(
                    new Vector2(tile.X * _tiledMap.TileWidth, tile.Y * _tiledMap.TileHeight),
                    Vector2.Zero,
                    new Vector2(_tiledMap.TileWidth, _tiledMap.TileHeight)));
            }
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var deltaTime = gameTime.GetElapsedSeconds();
            var collider = _colliderComponentMapper.Get(entityId);
            var physics = _physicsComponentMapper.Get(entityId);

            var obstacles = new List<Tuple<RectangularColliderComponent, float>>();
            var rectangularCollider = new RectangularColliderComponent(
                physics.Position,
                physics.Velocity,
                collider);

            //collider.PenetrationNormal = Vector2.Zero;
            //var bounds = (RectangleF) collider.Bounds;
            //collider.Bounds = new RectangleF(collider.Bounds);
            collider.Bounds.Position = physics.Position;
            _collisionComponent.Insert(collider);

            foreach (var obstacle in _obstacles)
            {
                var contactPoint = new Vector2();
                var contactNormal = new Vector2();
                var contactTime = 0.0f;
                if (rectangularCollider.Intersects(
                    obstacle,
                    ref contactPoint,
                    ref contactNormal,
                    ref contactTime,
                    deltaTime))
                        obstacles.Add(new Tuple<RectangularColliderComponent, float>(obstacle, contactTime));
            }

            if (obstacles.Count > 0)
            {
                // Do the sort
                obstacles.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                // Now resolve the collision in correct order
                foreach (var obstacle in obstacles)
                    rectangularCollider.Resolve(obstacle.Item1, deltaTime);
                
                //collider.hasCollided = true;
            }

            _collisionComponent.Update(gameTime);

            // Restrict movement
            Restrict(ref physics, ref collider);

            collider.PenetrationNormal = Vector2.Zero;
            collider.hasCollided = false;
            physics.Position = collider.Bounds.Position;
        }

        //public static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

        /*public bool Ray(
            ref RectangularColliderComponent origin,
            ref RectangularColliderComponent target,
            ref Vector2 contactPoint,
            ref Vector2 contactNormal,
            ref float timeHitNear,
            float deltaTime)
        {
            // Calculate ray vectors
            Vector2 rayOrigin = origin.Position + origin.Size / 2;
            Vector2 rayDirection = origin.Velocity * deltaTime;
            contactPoint = Vector2.Zero;
            contactNormal = Vector2.Zero;

            // Cache division
            var inverseDirection = Vector2.One / rayDirection;

            // Calculate intersections with rectangle boundings axes
            var targetNear = (target.Position - rayOrigin) * inverseDirection;
            var targetFar = (target.Position + target.Size - rayOrigin) * inverseDirection;

            if (float.IsNaN(targetFar.X) || float.IsNaN(targetFar.Y)) return false;
            if (float.IsNaN(targetNear.X) || float.IsNaN(targetNear.Y)) return false;

            // Sort distances
            if (targetNear.X > targetFar.X) Swap(ref targetNear.X, ref targetFar.X);
            if (targetNear.Y > targetFar.Y) Swap(ref targetNear.Y, ref targetFar.Y);

            // Early rejection
            if (targetNear.X > targetFar.Y || targetNear.Y > targetFar.X) return false;

            // Closest 'time' will be the first contact
            timeHitNear = MathF.Max(targetNear.X, targetNear.Y);

            // Furthest 'time' is contact on opposite side of target
            var timeHitFar = MathF.Min(targetFar.X, targetFar.Y);

            // Reject if ray directon is pointing away from object
            if (timeHitFar < 0.0f) return false;

            // Contact point of collision from parametric line equation
            contactPoint = rayOrigin + timeHitNear * rayDirection;

            if (targetNear.X > targetNear.Y)
                contactNormal = inverseDirection.X < 0 ? Vector2.UnitX : -Vector2.UnitX;
            else if (targetNear.X < targetNear.Y)
                contactNormal = inverseDirection.Y < 0 ? Vector2.UnitY : -Vector2.UnitY;

			// Note if targetNear == targetFar, collision is principly in a diagonal
			// so pointless to resolve. By returning a CN={0,0} even though its
			// considered a hit, the resolver wont change anything.

            return true;
        }

        // Collides?
        public bool Intersects(
            RectangularColliderComponent 
            RectangularColliderComponent obstacle,
            ref Vector2 contactPoint,
            ref Vector2 contactNormal,
            ref float contactTime,
            float deltaTime)
        {
            // Check if rectangle is actually moving - we assume rectangles are NOT in collision on start
            if (this.Velocity == Vector2.Zero) return false;

            // Expand target collider box by source dimensions
            var inflated = new RectangularColliderComponent(
                obstacle.Position - this.Size / 2,
                this.Velocity,
                obstacle.Size + this.Size);
            
            if (Ray(
                ref inflated,
                ref contactPoint,
                ref contactNormal,
                ref contactTime,
                deltaTime))
                    return (contactTime >= 0.0f && contactTime < 1.0f);
            else 
                return false;
        }

        public bool Resolve(
            RectangularColliderComponent obstacle,
            float deltaTime)
        {
            var contactPoint = new Vector2();
            var contactNormal = new Vector2();
            var contactTime = 0.0f;

            if (Intersects(
                obstacle,
                ref contactPoint,
                ref contactNormal,
                ref contactTime,
                deltaTime))
            {
                this.Contact[0] = contactNormal.Y > 0 ? obstacle : null;    // UP
                this.Contact[1] = contactNormal.X < 0 ? obstacle : null;    // LEFT
                this.Contact[2] = contactNormal.Y < 0 ? obstacle : null;    // DOWN
                this.Contact[3] = contactNormal.X > 0 ? obstacle : null;    // RIGHT

                this.Velocity += contactNormal * new Vector2(
                    MathF.Abs(this.Velocity.X),
                    MathF.Abs(this.Velocity.Y)) * (1 - contactTime);

                return true;
            }

            return false;
        }*/

        private static void Restrict(ref PhysicsComponent physics, ref ColliderComponent collider)
        {
            physics.Velocity = collider.PenetrationNormal == Vector2.UnitX ? new Vector2(
                MathHelper.Clamp(physics.Velocity.X, float.MinValue, 0),
                physics.Velocity.Y) : physics.Velocity;
            physics.Velocity = collider.PenetrationNormal == -Vector2.UnitX ? new Vector2(
                MathHelper.Clamp(physics.Velocity.X, 0, float.MaxValue), physics.Velocity.Y) : physics.Velocity;
            physics.Velocity = collider.PenetrationNormal == Vector2.UnitY ? new Vector2(
                physics.Velocity.X,
                 MathHelper.Clamp(physics.Velocity.Y, float.MinValue, 0)) : physics.Velocity;
            physics.Velocity = collider.PenetrationNormal == -Vector2.UnitY ? new Vector2(
                physics.Velocity.X, MathHelper.Clamp(physics.Velocity.Y, 0, float.MaxValue)) : physics.Velocity;
        }

        ~ColliderSystem() {}
    }
}