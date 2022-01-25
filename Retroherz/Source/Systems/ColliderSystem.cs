using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class ColliderSystem : EntityUpdateSystem
    {
        private readonly TiledMap _tiledMap;
        private readonly List<RectangularColliderComponent> _obstacles;
        private ComponentMapper<CircularColliderComponent> _circularColluderComponentMapper;
        private ComponentMapper<RectangularColliderComponent> _rectangularColliderComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public ColliderSystem(TiledMap tiledMap)
            : base(Aspect
                .All(typeof(PhysicsComponent))
                .One(typeof(CircularColliderComponent), typeof(RectangularColliderComponent)))
        {
            _tiledMap = tiledMap;
            _obstacles = new List<RectangularColliderComponent>();
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _circularColluderComponentMapper = mapperService.GetMapper<CircularColliderComponent>();
            _rectangularColliderComponentMapper = mapperService.GetMapper<RectangularColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();

                var tiles = _tiledMap.TileLayers[0].Tiles.Where(x => !x.IsBlank);
                foreach (var tile in tiles)
                {
                    var obstacle = RectangularColliderComponent.ToRectangularColliderComponent(tile, _tiledMap);
                    _obstacles.Add(obstacle);
                }
        }

        public override void Update(GameTime gameTime)
        {
            //var mouseState = Mouse.GetState();
            //var delta = gameTime.GetElapsedSeconds();
            var tiles = _tiledMap.TileLayers[0].Tiles.Where(x => !x.IsBlank);
            //var tiles = _tiledMap.GetLayer<TiledMapTileLayer>("Tile Layer 1");

            var deltaTime = gameTime.GetElapsedSeconds();
            foreach(var entityId in ActiveEntities)
            {
                var circularCollider = _circularColluderComponentMapper.Get(entityId);
                var collider = _rectangularColliderComponentMapper.Get(entityId);
                collider = collider == null ? new RectangularColliderComponent() : collider;
                var physics = _physicsComponentMapper.Get(entityId);
           
                // Curret location on grid
                var location = Vector2.Floor(collider.Position / new Vector2(
                    _tiledMap.TileWidth,
                    _tiledMap.TileHeight));

                // Sort collision in order of distance
                var contactPoint = new Vector2();
                var contactNormal = new Vector2();
                var contactTime = 0.0f;
                var obstacles = new List<Tuple<RectangularColliderComponent, float>>();

                // Work out collision point, add it to vector along with rectangle ID
                foreach (var obstacle in _obstacles)
                {
                    /*circularCollider.Velocity = physics.Velocity;
                    var size = new Vector2(_tiledMap.TileWidth, _tiledMap.TileHeight);
                    circularCollider.Intersects(tile, ref size, deltaTime);*/

                    //collider.Velocity = physics.Velocity;
                    //var obstacle = RectangularColliderComponent.ToRectangularColliderComponent(tile, _tiledMap);
                    if(collider.Intersects(
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

                    //System.Console.WriteLine(obstacles.Count);

                    // Now resolve the collision in correct order
                    foreach (var obstacle in obstacles)
                        collider.Resolve(obstacle.Item1, deltaTime);
                    
                    collider.hasCollided = true;
                }

                //circularCollider.Position += circularCollider.Velocity * deltaTime;
                
                // Update collider position with its current velocity
                collider.Position += collider.Velocity * deltaTime;
            }
        }
    }
}