using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;
using PubSub;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public enum ColliderSystemActions
    {

    }

    public class ColliderSystem : EntityUpdateSystem
    {
        Hub hub = Hub.Default;

        private readonly Bag<(ColliderComponent collider, TransformComponent transform)> _candidates;
        private readonly List<((ColliderComponent, TransformComponent) target, Vector2 contactPoint, Vector2 contactNormal, float contactTime)> _collisions;
        private readonly TiledMap _tiledMap;
        private readonly OrthographicCamera _camera;
        private readonly CollisionComponent _collisionsComponenet;
        private ComponentMapper<ColliderComponent> _colliderComponentMapper;
        private ComponentMapper<TransformComponent> _transformComponentMapper;

        internal float deltaTime;
        internal RectangleF inflated;
        internal int sourceId;
        internal Vector2 location;
        internal Vector2 inverseTileSize;
        internal IEnumerable<TiledMapTile> tiles;

        public ColliderSystem(TiledMap tiledMap, OrthographicCamera camera)
            : base(Aspect.All(typeof(ColliderComponent), typeof(TransformComponent)))
        {
            _tiledMap = tiledMap;
            _camera = camera;

            //_collisionsComponenet = new CollisionComponent(new RectangleF(0, 0, _tiledMap.WidthInPixels, _tiledMap.HeightInPixels));

            // Bag of value tuples to hold collision candidates
            _candidates = new Bag<(ColliderComponent, TransformComponent)>();

            // Sortable list of value tuples to hold de facto collisions to be resolved
            _collisions = new List<((ColliderComponent, TransformComponent), Vector2, Vector2, float)>();

            // Subscribe to map.
            hub.Subscribe<GetTiledMap>(this, payload => {
                //_tiledMap = payload.TiledMap;
            });
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _colliderComponentMapper = mapperService.GetMapper<ColliderComponent>();
            _transformComponentMapper = mapperService.GetMapper<TransformComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            deltaTime = gameTime.GetElapsedSeconds();

            // Filter
            var topLeft = _camera.BoundingRectangle.TopLeft * inverseTileSize;
            var bottomRight = _camera.BoundingRectangle.BottomRight * inverseTileSize;

            tiles = _tiledMap.TileLayers[0].Tiles.Where(tile => (
                !tile.IsBlank
                // Filters for camera viewport, but tiles will not exist for other entities!
                //&& (tile.X > topLeft.X && tile.X < bottomRight.X)
                //&& (tile.Y > topLeft.Y && tile.Y < bottomRight.Y)
            ));

            foreach (var entityId in ActiveEntities)
            {
                sourceId = entityId;
                var collider = _colliderComponentMapper.Get(entityId);
                var transform = _transformComponentMapper.Get(entityId);

                // Cache division
                inverseTileSize = Vector2.One / new Vector2(
                    _tiledMap.TileWidth,
                    _tiledMap.TileHeight);
        
                // Curret location on grid
                location = Vector2.Floor(transform.Position * inverseTileSize);

                // Restrict movement to map grid
                if (transform.Position.X < 0)
                {
                    transform.Position = transform.Position.SetX(0);
                    collider.Velocity = collider.Velocity.SetX(0);
                }
                else if (transform.Position.X > _tiledMap.WidthInPixels - collider.Size.X)
                {
                    transform.Position = transform.Position.SetX(_tiledMap.WidthInPixels - collider.Size.X);
                    collider.Velocity = collider.Velocity.SetX(0);
                }
                else if (transform.Position.Y < 0)
                {
                    transform.Position = transform.Position.SetY(0);
                    collider.Velocity = collider.Velocity.SetY(0);
                }
                else if (transform.Position.Y > _tiledMap.HeightInPixels - collider.Size.Y)
                {
                    transform.Position = transform.Position.SetY(_tiledMap.HeightInPixels - collider.Size.Y);
                    collider.Velocity = collider.Velocity.SetY(0);
                }

                // All dynamic colliders should be rounded to nearest integer to help with
                // moving through tight passages etc.
                // Tiles are static and can be ignored in this regard.

                // Bounding rectangle
                var bounds = new RectangleF(Vector2.Round(transform.Position), collider.Size);

                // Pilot rectangle
                var pilot = new RectangleF(transform.Position + collider.Velocity * deltaTime, collider.Size);

                // Inflated rectangle (search area)
                var minimum = Vector2.Min(bounds.TopLeft, pilot.TopLeft);
                var maximum = Vector2.Max(bounds.BottomRight, pilot.BottomRight);
                inflated = new RectangleF(minimum, maximum - minimum);

                // Clear and add colliders
                _candidates.Clear();
                AddStaticColliders();
                AddDynamicColliders();

                // Sort collision in order of distance
                var contactPoint = Vector2.Zero;
                var contactNormal = Vector2.Zero;
                var contactTime = 0.0f;

                // Work out collision point ...
                collider.ContactInfo.Clear();
                _collisions.Clear();
                foreach (var candidate in _candidates)
                { 
                    if (Collides(
                        (collider, transform),
                        candidate,
                        out contactPoint,
                        out contactNormal,
                        out contactTime,
                        deltaTime))
                    {
                        // ... add it to vector along with rectangle ID
                        _collisions.Add(((candidate, contactPoint, contactNormal, contactTime)));

                        // ... and contact information for visuals etc.
                        collider.ContactInfo.Add((candidate, contactPoint, contactNormal, contactTime));
                    }
                }

                if (_collisions.Count > 0)
                {
                    // Do the sort
                    _collisions.Sort((a, b) => a.contactTime < b.contactTime ? -1 : 1);

                    // Now resolve the collision in correct order
                    foreach (var collision in _collisions)
                    {
                        if(ResolveCollision((collider, transform), collision.target, deltaTime));
                            /*hub.Publish<TiledMapSystemEvent>(new TiledMapSystemEvent(
                                TiledMapSystemAction.RemoveTile,
                                collider.Item1.Position));*/
                    }
                }
            }
        }

        internal void AddStaticColliders()
        {
            // Inspect tile area
            /*var offset = Vector2.Round(collider.Size * inverseTileSize);

            // Counter outer of bounds
            if (location.X <= 0) location = location.SetX(1);
            if (location.Y <= 0) location = location.SetY(1);

            // Account for entity size
            var offsetX = offset.X >= 1 ? ((ushort)offset.X) : 1;
            var offsetY = offset.Y >= 1 ? ((ushort)offset.Y) : 1;

            // We want to check each tile area with a padding of one from entity
            var X = ((ushort)(MathF.Round(location.X - 1)));
            var Y = ((ushort)(MathF.Round(location.Y - 1)));
            var rangeX = ((ushort)(X + offsetX + 2));
            var rangeY = ((ushort)(Y + offsetY + 2));

            var tileLayer = _tiledMap.TileLayers[0];
            var count = 0;
            for (ushort y = Y; y <= rangeY; y++)
                for (ushort x = X; x <= rangeX; x++)
                {
                    var tile = tileLayer.GetTile(x, y);
                    if (!tile.IsBlank)
                    {
                        var position = new Vector2(tile.X * _tiledMap.TileWidth, tile.Y * _tiledMap.TileHeight);
                        var size = new Vector2(_tiledMap.TileWidth, _tiledMap.TileHeight);
                        _candidates.Add((
                            new ColliderComponent(size: size, type: ColliderComponentType.Static),
                            new TransformComponent(position: position)));
                        count++;
                    }
                }
            System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}", count, X, rangeX, Y, rangeY);*/

            foreach (var tile in tiles)
            {
                var position = new Vector2(tile.X * _tiledMap.TileWidth, tile.Y * _tiledMap.TileHeight);
                var size = new Vector2(_tiledMap.TileWidth, _tiledMap.TileHeight);
                var candidate = new RectangleF(position, size);

                if(inflated.Intersects(candidate))
                    _candidates.Add((
                        new ColliderComponent(size: size, type: ColliderComponentType.Static),
                        new TransformComponent(position: position)));
            }
        }

        internal void AddDynamicColliders()
        {
            var topLeft = _camera.BoundingRectangle.TopLeft;
            var bottomRight = _camera.BoundingRectangle.BottomRight;

            foreach (var targetId in ActiveEntities)
            {
                var collider = _colliderComponentMapper.Get(targetId);
                var transform = _transformComponentMapper.Get(targetId);

                // Conditionals
                var windowX = transform.Position.X > topLeft.X - collider.Size.X && transform.Position.X < bottomRight.X;
                var windowY = transform.Position.Y > topLeft.Y - collider.Size.Y && transform.Position.Y < bottomRight.Y;

                // Add if within view
                if (inflated.Intersects(new RectangleF(transform.Position, collider.Size)) && targetId != sourceId)
                //if (windowX && windowY && entityId != test)
                    _candidates.Add((collider, transform));
            }
        }

        ~ColliderSystem() { hub.Unsubscribe<GetTiledMap>(); }

        // Courtesy of One Lone Coder
        // https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

        internal bool Intersects(
            Vector2 rayOrigin,
            Vector2 rayDirection,
            (ColliderComponent collider, TransformComponent transform) target,
            out Vector2 contactPoint,
            out Vector2 contactNormal,
            out float timeHitNear)
        {
            contactPoint = Vector2.Zero;
            contactNormal = Vector2.Zero;
            timeHitNear = 0.0f;

            // Are we in the correct quadrant?
            var outOfBounds = target.transform.Position.X < 0 || target.transform.Position.Y < 0;

            // To not confuse the algorithm we shift target position and ray origin
            // to force upcomming calculations over to the positiv axes.
            // - VE
            if (outOfBounds)
            {
                target.transform.Position += target.collider.Size;
                rayOrigin += target.collider.Size;
            }

             // Cache division
            var inverseDirection = Vector2.One / rayDirection;

            // Calculate intersections with rectangle boundings axes
            var targetNear = (target.transform.Position - rayOrigin) * inverseDirection;
            var targetFar = (target.transform.Position + target.collider.Size - rayOrigin) * inverseDirection;
          
            if (float.IsNaN(targetFar.X) || float.IsNaN(targetFar.Y)) return false;
            if (float.IsNaN(targetNear.X) || float.IsNaN(targetNear.Y)) return false;

            // Sort distances
            if (targetNear.X > targetFar.X) Swap(ref targetNear.X, ref targetFar.X);
            if (targetNear.Y > targetFar.Y) Swap(ref targetNear.Y, ref targetFar.Y);

            // Early rejection
            if (targetNear.X > targetFar.Y || targetNear.Y > targetFar.X) return false;

            // Closest 'time' will be the first contact
            timeHitNear = MathF.Max(targetNear.X, targetNear.Y);

            // For a correct calculation of contact point we shift ray origin back
            // - VE
            if (outOfBounds) rayOrigin -= target.collider.Size;

            // Furthest 'time' is contact on opposite side of target
            var timeHitFar = MathF.Min(targetFar.X, targetFar.Y);
            
            // Reject if ray directon is pointing away from object (can be usefull)
            if (timeHitFar < 0) return false;

            // Contact point of collision from parametric line equation
            contactPoint = rayOrigin + timeHitNear * rayDirection;

            if (targetNear.X > targetNear.Y)
                contactNormal = inverseDirection.X < 0 ? Vector2.UnitX : -Vector2.UnitX;
            else if (targetNear.X < targetNear.Y)
                contactNormal = inverseDirection.Y < 0 ? Vector2.UnitY : -Vector2.UnitY;
                // Note if targetNear == targetFar, collision is principly in a diagonal
                // so pointless to resolve. By returning a CN={0,0} even though its
                // considered a hit, the resolver wont change anything.

            else
                // Set (proper) contacts for diagonal collisions
                // - VE
                if (inverseDirection.X < 0 && inverseDirection.Y > 0) contactNormal = new Vector2(1, -1);
                else if (inverseDirection.X < 0 && inverseDirection.Y < 0) contactNormal = new Vector2(1, 1);
                else if (inverseDirection.X > 0 && inverseDirection.Y > 0) contactNormal = new Vector2(-1, -1);
                else if (inverseDirection.X > 0 && inverseDirection.Y < 0) contactNormal = new Vector2(-1, 1);
                
            // Resolve diagonal collision by reducing contact normal to one direction (seems to work nicely)
            // - VE
            if (contactNormal.X != 0 && contactNormal.Y != 0)
                if (contactNormal.X > 0) contactNormal = contactNormal.SetY(0);
                else if (contactNormal.Y > 0) contactNormal = contactNormal.SetX(0);
                else contactNormal = Random.Shared.NextSingle() > 0.5f ? contactNormal.SetX(0) : contactNormal.SetY(0);

            return true;
        }

        internal bool Collides(
            (ColliderComponent collider, TransformComponent transform) source,
            (ColliderComponent collider, TransformComponent transform) target,
            out Vector2 contactPoint,
            out Vector2 contactNormal,
            out float contactTime,
            float deltaTime)
        {
            contactPoint = Vector2.Zero;
            contactNormal = Vector2.Zero;
            contactTime = 0.0f;
               
            // Check if rectangle is actually moving - we assume rectangles are NOT in collision on start
            if (source.collider.Velocity == Vector2.Zero) return false;
            
            // Expand target collider box by source dimensions
            (ColliderComponent collider, TransformComponent transform) inflatedTarget = (
                new ColliderComponent(
                    size: target.collider.Size + source.collider.Size,
                    velocity: target.collider.Velocity,
                    type: target.collider.Type),
                new TransformComponent(position: target.transform.Position - source.collider.Origin));

            // EXP
            if (target.collider.Type == ColliderComponentType.Dynamic)
            {
                // ???
                var sourceRectangle = new RectangleF(source.transform.Position + source.collider.Velocity * deltaTime, source.collider.Size);
                var targetRectangle = new RectangleF(target.transform.Position + target.collider.Velocity * deltaTime, target.collider.Size);

                var intersectingRectangle = sourceRectangle.Intersection(targetRectangle);

                // Early rejection
                if (intersectingRectangle.IsEmpty) return false;

                Vector2 penetration;
                if (intersectingRectangle.Width < intersectingRectangle.Height)
                {
                    var displacement = sourceRectangle.Center.X < targetRectangle.Center.X
                        ? intersectingRectangle.Width
                        : -intersectingRectangle.Width;
                    penetration = new Vector2(displacement, 0);
                }
                else
                {
                    var displacement = sourceRectangle.Center.Y < targetRectangle.Center.Y
                        ? intersectingRectangle.Height
                        : -intersectingRectangle.Height;
                    penetration = new Vector2(0, displacement);
                }

                contactNormal = -penetration.NormalizedCopy();
                if (float.IsNaN(contactNormal.X) || float.IsNaN(contactNormal.Y)) contactNormal = Vector2.Zero;

                contactPoint = source.transform.Position + source.collider.Origin;
                contactTime = (penetration / target.collider.Size).Length();

                // FUN
                //target.collider.Velocity += -contactNormal * new Vector2(MathF.Abs(source.collider.Velocity.X), MathF.Abs(source.collider.Velocity.Y) * contactTime);
                //source.collider.Velocity += contactNormal * new Vector2(MathF.Abs(target.collider.Velocity.X), MathF.Abs(target.collider.Velocity.Y) * contactTime);

                return (contactNormal != Vector2.Zero) ? true : false;
            }

            // Calculate ray vectors
            var rayOrigin = source.transform.Position + source.collider.Origin;
            var rayDirection = source.collider.Velocity * deltaTime;
         
            // Cast ray
            if (Intersects(
                rayOrigin,
                rayDirection,
                inflatedTarget,
                out contactPoint,
                out contactNormal,
                out contactTime))
                    return (contactTime >= 0.0f && contactTime < 1f);
            else 
                return false;
        }

        internal bool ResolveCollision(
            (ColliderComponent collider, TransformComponent transform) source,
            (ColliderComponent collider, TransformComponent transform) target,
            float deltaTime)
        {
            var contactPoint = Vector2.Zero;
            var contactNormal = Vector2.Zero;
            var contactTime = 0.0f;

            if (Collides(
                source,
                target,
                out contactPoint,
                out contactNormal,
                out contactTime,
                deltaTime))
            {
                /*if (target.collider.Type == ColliderComponentType.Dynamic)
                {
                    target.collider.Velocity = -contactNormal * new Vector2(
                    MathF.Abs(source.collider.Velocity.X),
                    MathF.Abs(source.collider.Velocity.Y));// * (1 - contactTime));

                    source.collider.Velocity = -contactNormal * new Vector2(
                    MathF.Abs(target.collider.Velocity.X),
                    MathF.Abs(target.collider.Velocity.Y));// * (1 - contactTime));
                }*/

                // Add contact normal information
                source.collider.Contact[0] = contactNormal.Y > 0 ? target : null;
                source.collider.Contact[1] = contactNormal.X < 0 ? target : null;
                source.collider.Contact[2] = contactNormal.Y < 0 ? target : null;
                source.collider.Contact[3] = contactNormal.X > 0 ? target : null;

                // Displace
                source.collider.Velocity += contactNormal * new Vector2(
                    MathF.Abs(source.collider.Velocity.X),
                    MathF.Abs(source.collider.Velocity.Y)) * (1 - contactTime);

                return true;
            }

            return false;
        }
    }
}