using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;
using PubSub;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public enum PhysicsSystemActions
    {

    }

    public class PhysicsSystem : EntityProcessingSystem
    {
        Hub hub = Hub.Default;

        public bool isAnomaly = false;

        private readonly Bag<PhysicsComponent> _colliders;
        private TiledMap _tiledMap;
        private ComponentMapper<ColliderComponent> _colliderCompomentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public PhysicsSystem(TiledMap tiledMap)
            : base(Aspect
                .All(typeof(PhysicsComponent)))
        {
            //_tiledMap = tiledMap;
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

            // Work out collision point ...
            physics.ContactInfo.Clear();
            foreach (var collider in _colliders)
            {                                 
                if (Collides(
                    ref physics,
                    collider,
                    ref contactPoint,
                    ref contactNormal,
                    ref contactTime,
                    deltaTime))
                    {
                        // ... add it to vector along with rectangle ID
                        colliders.Add(new Tuple<PhysicsComponent, float>(collider, contactTime));

                        // ... and contact information for visuals etc.
                        var contact = new PhysicsComponent(
                            position: collider.Position,
                            size: collider.Size);
                        physics.ContactInfo.Add(new Tuple<PhysicsComponent, Vector2, Vector2, float>(
                            contact, contactPoint, contactNormal, contactTime));
                   }
            }

            if (colliders.Count > 0)
            {
                // Do the sort
                //colliders.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                colliders.Sort((a, b) => a.Item2 < b.Item2 ? -1 : 1);

                // Now resolve the collision in correct order
                foreach (var collider in colliders)
                {
                    if(ResolveCollision(physics, collider.Item1, deltaTime));/*
                        hub.Publish<TiledMapSystemEvent>(new TiledMapSystemEvent(
                            TiledMapSystemAction.RemoveTile,
                            collider.Item1.Position));*/
                }
            }

            // Remove Tile
            /*foreach (var normal in physics.Contact)
                if (normal != null)
                    hub.Publish<TiledMapSystemEvent>(new TiledMapSystemEvent(
                        TiledMapSystemAction.RemoveTile,
                        normal.Position));*/

            // Update position
            physics.Position += physics.Velocity * deltaTime;
        }

        private void EvaluateColliders(PhysicsComponent physics, float deltaTime)
        {
            // Bounding rectangle
            // Roundes of to prevent errors furter down the line
            // THIIS!!!!!!! New errors => acceleration
            // Finaly we can pass an enrance!! Requires tiled worlds. Good stuff really.
            var bounds = new RectangleF(Vector2.Round(physics.Position), physics.Size);

            // Pilot rectangle
            var pilot = new RectangleF(physics.Position + physics.Velocity * deltaTime * physics.Size, physics.Size);

            // Inflated rectangle (search area)
            var minimum = Vector2.Min(bounds.TopLeft, pilot.TopLeft);
            var maximum = Vector2.Max(bounds.BottomRight, pilot.BottomRight);
            var inflated = new RectangleF(minimum, maximum - minimum);

            // Hack?
            //inflated.Inflate(1f, 1f);

            // Load tiles
            var tiles = _tiledMap.TileLayers[0].Tiles.Where(x => !x.IsBlank);     

            // Clear and add colliders
            _colliders.Clear();
            foreach (var tile in tiles)
            {
                var position = new Vector2(tile.X * _tiledMap.TileWidth, tile.Y * _tiledMap.TileHeight);
                var size = new Vector2(_tiledMap.TileWidth, _tiledMap.TileHeight);
                var candidate = new RectangleF(position, size);
                var wall = new PhysicsComponent(position: position, size: size, type: PhysicsComponentType.Static);

                if(inflated.Intersects(candidate))
                    _colliders.Add(wall);
            }

            // Add actors
            foreach (var entityId in ActiveEntities)
            {
                var entity = _physicsComponentMapper.Get(entityId);
                var candidate = new RectangleF(Vector2.Round(entity.Position), entity.Size);
                entity.Type = PhysicsComponentType.Dynamic;

                // Ignore self (does it do anything?)
                if(!physics.Equals(entity) && inflated.Intersects(candidate))
                    _colliders.Add(entity);
            }
        }

        ~PhysicsSystem() {}

        // Courtesy of One Lone Coder
        // https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp
        
        private static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

        private bool Intersects(
            Vector2 rayOrigin,
            Vector2 rayDirection,
            PhysicsComponent target,
            ref Vector2 contactPoint,
            ref Vector2 contactNormal,
            ref float timeHitNear)
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
            
            //System.Console.WriteLine("{0}, {1}", timeHitNear, timeHitFar);

            // Reject if ray directon is pointing away from object (can be usefull)
            if (timeHitFar < 0) return false;

            // This tooo??? MUST BE APPLIED!
            // Seems to help with weird through-walls cases (now sticks to wall :/)
            if (timeHitNear < 0) timeHitNear = 0;

            // Contact point of collision from parametric line equation
            contactPoint = rayOrigin + timeHitNear * rayDirection;

            // Anomally happpens            
            /*if (timeHitNear > 1)
            {
                timeHitNear = float.MinValue;
                System.Console.WriteLine("Ouch!");
            }*/

            if (targetNear.X > targetNear.Y)
                contactNormal = inverseDirection.X < 0 ? Vector2.UnitX : -Vector2.UnitX;
                /*if (inverseDirection.X < 0) contactNormal = new Vector2(1, 0);
                else contactNormal = new Vector2(-1, 0);*/
            else if (targetNear.X < targetNear.Y)
                contactNormal = inverseDirection.Y < 0 ? Vector2.UnitY : -Vector2.UnitY;
                /*if (inverseDirection.Y < 0) contactNormal = new Vector2(0, 1);
                else contactNormal = new Vector2(0, -1);*/

            System.Console.WriteLine("{0}, {1}, {2}. {3}, {4}, {5}", timeHitNear, timeHitFar, rayOrigin, contactPoint, contactNormal, targetNear == targetFar);

			// Note if targetNear == targetFar, collision is principly in a diagonal
			// so pointless to resolve. By returning a CN={0,0} even though its
			// considered a hit, the resolver wont change anything.

            return true;
        }

        private bool Collides(
            ref PhysicsComponent collider,
            PhysicsComponent obstacle,
            ref Vector2 contactPoint,
            ref Vector2 contactNormal,
            ref float contactTime,
            float deltaTime)
        {
            //contactTime = 0f;
            // Check if rectangle is actually moving - we assume rectangles are NOT in collision on start
            if (collider.Velocity == Vector2.Zero) return false;

            // Slight displacement to hinder collider from getting wedged between tiles
            //var hack = 0.9999f;

            // Expand target collider box by source dimensions
            var inflated = new PhysicsComponent(
                position: obstacle.Position - collider.Origin,// * hack,
                size: obstacle.Size + collider.Size);// * hack);

            // Calculate ray vectors
            var rayOrigin = collider.Position + collider.Origin;
            var rayDirection = collider.Velocity * deltaTime;
          
            // Cast ray
            if (Intersects(
                rayOrigin,
                rayDirection,
                inflated,
                ref contactPoint,
                ref contactNormal,
                ref contactTime))
                    return (contactTime >= 0.0f && contactTime < 1f);
            else 
                return false;
        }

        private bool ResolveCollision(
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

                var difference = (collider.Position - contactPoint) + collider.Origin;
                //difference = Vector2.Zero;
                // Apply anomalic difference if present (to remedy anomaly)
                //collider.Velocity += new Vector2(intersection.Width, intersection.Height);

                // THIS!!!!!!??! But has jumping step over effect gaps.
                //if (contactTime < 0)
                    //collider.Position = Vector2.Round(collider.Position);

                // Weird :P
                //if ((collider.Position.X % 8 != 0) || (collider.Position.Y % 8 != 0))
                  //  { collider.Position = Vector2.Round(collider.Position); System.Console.WriteLine("SSHOUT!"); }

                // ?????
                //collider.Velocity += new Vector2(contactNormal.X * intersection.X, contactNormal.Y * intersection.Y);// * deltaTime;

                // Calculate displacement vector
                collider.Velocity += contactNormal * new Vector2(
                    MathF.Abs(collider.Velocity.X),
                    MathF.Abs(collider.Velocity.Y)) * (1 - contactTime);


                System.Console.WriteLine("{0}, {1}, {2}. {3}", contactTime, collider.Origin, contactPoint, contactNormal);

                return true;
            }

            return false;
        }
    }
}