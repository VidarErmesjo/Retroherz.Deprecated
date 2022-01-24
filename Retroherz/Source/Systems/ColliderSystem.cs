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
        private ComponentMapper<RectangularColliderComponent> _rectangularColliderComponentMapper;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;

        public ColliderSystem(TiledMap tiledMap)
            : base(Aspect.All(typeof(RectangularColliderComponent), typeof(PhysicsComponent)))
        {
            _tiledMap = tiledMap;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _rectangularColliderComponentMapper = mapperService.GetMapper<RectangularColliderComponent>();
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
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
                var collider = _rectangularColliderComponentMapper.Get(entityId);
                var physics = _physicsComponentMapper.Get(entityId);
           
                // Curret location on grid
                var location = Vector2.Floor(collider.Position / new Vector2(
                    _tiledMap.TileWidth,
                    _tiledMap.TileHeight));

               /* var tile = tiles.GetTile(((ushort)location.X), ((ushort)location.Y));

                if(tile.GlobalIdentifier == 1)
                    System.Console.WriteLine("{0}", tile.ToString());*/

                // Sort collision in order of distance
                var contactPoint = new Vector2();
                var contactNormal = new Vector2();
                var contactTime = 0.0f;
                var obstacles = new List<Tuple<RectangularColliderComponent, float>>();

                // Work out collision point, add it to vector along with rectangle ID
                foreach (var tile in tiles)
                {
                    //collider.Position = physics.Position;
                    collider.Velocity = physics.Velocity;
                    var obstacle = RectangularColliderComponent.ToRectangularColliderComponent(tile, _tiledMap);
                    if(collider.Intersects(ref obstacle, deltaTime, ref contactPoint, ref contactNormal, ref contactTime))
                        obstacles.Add(new Tuple<RectangularColliderComponent, float>(obstacle, contactTime));
                }

                // Do the sort
                obstacles.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                // Now resolve the collision in correct order
                foreach (var obstacle in obstacles)
                    collider.Resolve(obstacle.Item1, deltaTime);

                collider.Position += collider.Velocity * deltaTime;
            }
        }
    }
}