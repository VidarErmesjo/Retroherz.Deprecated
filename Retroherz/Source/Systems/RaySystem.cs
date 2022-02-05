using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Tiled;
using PubSub;

using Retroherz.Components;

namespace Retroherz.Systems
{
    public class RaySystem : EntityProcessingSystem
    {
        private TiledMap _tiledMap;
        private Hub hub = Hub.Default;
        private ComponentMapper<PhysicsComponent> _physicsComponentMapper;
        private ComponentMapper<RayComponent> _rayComponentMapper;

        public RaySystem()
            : base(Aspect.All(typeof(PhysicsComponent), typeof(RayComponent)))
        {
            hub.Subscribe<GetTiledMap>(this, payload => {
                _tiledMap = payload.TiledMap;
            });
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _physicsComponentMapper = mapperService.GetMapper<PhysicsComponent>();
            _rayComponentMapper = mapperService.GetMapper<RayComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var deltaTime = gameTime.GetElapsedSeconds();
            var mouseState = Mouse.GetState();
            var physics = _physicsComponentMapper.Get(entityId);
            var ray = _physicsComponentMapper.Get(entityId);

            // DDA Algorithm ==============================================
		    // https://lodev.org/cgtutor/raycasting.html

            Vector2 mouseCell = new Vector2(mouseState.X, mouseState.Y) / new Vector2(
                _tiledMap.TileWidth,
                _tiledMap.TileHeight);

            Size2 cell = mouseCell;

            // Form ray cast from player into scene
            ray.Position = physics.Position;
            ray.Direction = physics.Direction; //Vector2.Normalize(mouseCell - physics.Position);

            // Lodev.org also explains this additional optimistaion (but it's beyond scope of video)
            // olc::vf2d vRayUnitStepSize = { abs(1.0f / vRayDir.x), abs(1.0f / vRayDir.y) };
            Vector2 rayUnitStepSize = new Vector2(
                MathF.Sqrt(1 + (ray.Direction.Y / ray.Direction.X) * (ray.Direction.Y / ray.Direction.X)),
                MathF.Sqrt(1 + (ray.Direction.X / ray.Direction.Y) * (ray.Direction.X / ray.Direction.Y)));

            Vector2 mapCheck = ray.Origin;
            Vector2 rayLength1D;
            Vector2 step;

        // Establish starting conditions
        if (ray.Direction.X < 0)
        {
            step.X = -1;
            rayLength1D.X = (ray.Origin.X - mapCheck.X) * rayUnitStepSize.X; 
        }
        else
        {
            step.X = 1;
            rayLength1D.X = (((byte)(mapCheck.X + 1) ) * ray.Origin.X) * rayUnitStepSize.X;
        }

        if (ray.Direction.Y < 0)
        {
            step.Y = -1;
            rayLength1D.Y = (ray.Origin.Y - mapCheck.Y) * rayUnitStepSize.Y;
        }
        else
        {
            step.Y = 1;
            rayLength1D.Y = ((mapCheck.Y + 1) - ray.Origin.Y) * rayUnitStepSize.Y;
        }

        // Perform "Walk" until collision or range check
        bool tileFound = false;
        float maxDistance = 100.0f;
        float distance = 0.0f;

        while (!tileFound && distance < maxDistance)
        {
            if (rayLength1D.X < rayLength1D.Y)
            {
                mapCheck.X += step.X;
                distance = rayLength1D.X;
                rayLength1D.X += rayUnitStepSize.X;
            }
            else
            {
                mapCheck.Y += step.Y;
                distance = rayLength1D.Y;
                rayLength1D.Y += rayUnitStepSize.Y;
            }

            // Test tile at new test point
            //if (mapCheck.X >= 0 && mapCheck.X < tiledMapSyste)
        }
/*
			// Test tile at new test point
			if (vMapCheck.x >= 0 && vMapCheck.x < vMapSize.x && vMapCheck.y >= 0 && vMapCheck.y < vMapSize.y)
			{
				if (vecMap[vMapCheck.y * vMapSize.x + vMapCheck.x] == 1)
				{
					bTileFound = true;
				}
			}
		}
*/

        }

        ~RaySystem() {}
    }
}