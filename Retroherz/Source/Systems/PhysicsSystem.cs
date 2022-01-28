using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class PhysicsSystem : EntityProcessingSystem
    {
        private readonly TiledMap _tiledMap;
        private readonly Bag<PhysicsComponent> _colliders;
        private ComponentMapper<ColliderComponent> _colliderCompomentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public PhysicsSystem(TiledMap tiledMap)
            : base(Aspect
                .All(typeof(PhysicsComponent)))
        {
            _tiledMap = tiledMap;
            _colliders = new Bag<PhysicsComponent>();
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderCompomentMapper = mapperService.GetMapper<ColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var deltaTime = gameTime.GetElapsedSeconds();

            //var collider = _colliderCompomentMapper.Get(entityId);
            var physics = _physicsComponentMapper.Get(entityId);
      
            // Curret location on grid
            var location = Vector2.Floor(physics.Position / new Vector2(
                _tiledMap.TileWidth,
                _tiledMap.TileHeight));

            // Get a subset of colliders
            EvaluateColliders(physics, deltaTime);

            // Sort collision in order of distance
            var contactPoint = new Vector2();
            var contactNormal = new Vector2();
            var contactTime = new float();
            var colliders = new List<Tuple<PhysicsComponent, float>>(_colliders.Count);

            // Work out collision point, add it to vector along with rectangle ID
            foreach (var collider in _colliders)
            {                                 
                if (Collides(
                    ref physics,
                    collider,
                    ref contactPoint,
                    ref contactNormal,
                    ref contactTime,
                    deltaTime))
                        colliders.Add(new Tuple<PhysicsComponent, float>(collider, contactTime));
            }

            if (colliders.Count > 0)
            {
                // Do the sort
                colliders.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                // Now resolve the collision in correct order
                foreach (var collider in colliders)
                    ResolveCollision(physics, collider.Item1, deltaTime);
            }

            // Update position
            physics.Position += physics.Velocity * deltaTime;
        }

        private void EvaluateColliders(PhysicsComponent physics, float deltaTime)
        {
            // Predict search area
            var inflated = new RectangleF(physics.Position, physics.Size);
            var factor = physics.Velocity.NormalizedCopy() * physics.Size * physics.Velocity.Length() * deltaTime; //* physics.Size.Length() * deltaTime;
            inflated.Inflate(MathF.Abs(factor.X), MathF.Abs(factor.Y));

            // Load tiles
            var tiles = _tiledMap.TileLayers[0].Tiles.Where(x => !x.IsBlank);

            // Clear and add self
            _colliders.Clear();
            foreach (var tile in tiles)
            {
                var position = new Vector2(tile.X * _tiledMap.TileWidth, tile.Y * _tiledMap.TileHeight);
                var size = new Vector2(_tiledMap.TileWidth, _tiledMap.TileHeight);
                var candidate = new RectangleF(position, size);

                if(inflated.Intersects(candidate))
                    _colliders.Add(new PhysicsComponent(position: position, size: size));
            }

            // Add actors
            foreach (var entityId in ActiveEntities)
            {
                var entity = _physicsComponentMapper.Get(entityId);
                var candidate = new RectangleF(entity.Position, entity.Size);

                if(!physics.Equals(entity) && inflated.Intersects(candidate))
                    _colliders.Add(new PhysicsComponent(position: entity.Position, size: entity.Size));
            }
        }

        // Courtesy of One Lone Coder
        // https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp
        
        private static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

        private static bool Intersects(
            Vector2 rayOrigin,
            Vector2 rayDirection,
            PhysicsComponent target,
            ref Vector2 contactPoint,
            ref Vector2 contactNormal,
            ref float timeHitNear,
            float deltaTime)
        {
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

        private static bool Collides(
            ref PhysicsComponent collider,
            PhysicsComponent obstacle,
            ref Vector2 contactPoint,
            ref Vector2 contactNormal,
            ref float contactTime,
            float deltaTime)
        {
            // Check if rectangle is actually moving - we assume rectangles are NOT in collision on start
            if (collider.Velocity == Vector2.Zero) return false;

            // Slight displacement to hinder collider from getting wedged between tiles
            var hack = 0.9999f;

            // Expand target collider box by source dimensions
            var inflated = new PhysicsComponent(
                position: obstacle.Position - collider.Size / 2,// * hack,
                size: obstacle.Size + collider.Size);// * hack);

            // Calculate ray vectors
            var rayOrigin = collider.Position + collider.Size / 2;
            var rayDirection = collider.Velocity * deltaTime;
            
            // Cast ray
            if (Intersects(
                rayOrigin,
                rayDirection,
                inflated,
                ref contactPoint,
                ref contactNormal,
                ref contactTime,
                deltaTime))
                    return (contactTime >= 0.0f && contactTime < 1.0f);
            else 
                return false;
        }

        private static bool ResolveCollision(
            PhysicsComponent collider,
            PhysicsComponent obstacle,
            float deltaTime)
        {
            var contactPoint = Vector2.Zero;
            var contactNormal = Vector2.Zero;
            var contactTime = 0.0f;

            if (Collides(
                ref collider,
                obstacle,
                ref contactPoint,
                ref contactNormal,
                ref contactTime,
                deltaTime))
            {
                // Add contact normal information
                collider.Contact[0] = contactNormal.Y > 0 ? obstacle : null;
                collider.Contact[1] = contactNormal.X < 0 ? obstacle : null;
                collider.Contact[2] = contactNormal.Y < 0 ? obstacle : null;
                collider.Contact[3] = contactNormal.X > 0 ? obstacle : null;

                // Calculate displacement vector
                collider.Velocity += contactNormal * new Vector2(
                    MathF.Abs(collider.Velocity.X),
                    MathF.Abs(collider.Velocity.Y)) * (1 - contactTime);

                return true;
            }

            return false;
        }
    }
}