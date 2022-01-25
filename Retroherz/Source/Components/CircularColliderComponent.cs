using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

namespace Retroherz.Components
{
    public class CircularColliderComponent
    {
        private Vector2 _potentialPosition = new Vector2();
        private Vector2 _rayToNearest = new Vector2();
        private Vector2 _nearestPoint = new Vector2();
    
        public bool hasCollided = false;
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Radius { get; set; }

        public CircularColliderComponent(
            Vector2 position = default(Vector2),
            Vector2 velocity = default(Vector2),
            float radius = 1.0f)
        {
            Position = position;
            Velocity = velocity;
            Radius = radius;
        }

        public bool Intersects(TiledMapTile tile, ref Vector2 size, float deltaTime)
        {
            _potentialPosition = _rayToNearest = _nearestPoint = new Vector2();

            // Where will the object be worst case?
            _potentialPosition += this.Position + this.Velocity * deltaTime; 

            /*
		// Extract region of world cells that could have collision this frame
		olc::vi2d vCurrentCell = object.vPos.floor();
		olc::vi2d vTargetCell = vPotentialPosition;
		olc::vi2d vAreaTL = (vCurrentCell.min(vTargetCell) - olc::vi2d(1, 1)).max({ 0,0 });
		olc::vi2d vAreaBR = (vCurrentCell.max(vTargetCell) + olc::vi2d(1, 1)).min(vWorldSize);
            */

            // Conver tiles to cell array in the future? TiledSystem.cs ??
            _nearestPoint.X = MathF.Max(tile.X * size.X, MathF.Min(_potentialPosition.X, (tile.X + 1) * size.X));
            _nearestPoint.Y = MathF.Max(tile.Y * size.Y, MathF.Min(_potentialPosition.Y, (tile.Y + 1) * size.Y));

            _rayToNearest = _nearestPoint - _potentialPosition;
            float overlap = this.Radius - _rayToNearest.Length();

            // Division by zero?
            if (float.IsNaN(overlap)) overlap = 0;

            // If overlap is positive, then a collision has occurred, so we displace backwards by the 
            // overlap amount. The potential position is then tested against other tiles in the area
            // therefore "statically" resolving the collision
            if (overlap > 0)
            {
                // Set the objects new position to the allowed potential position
                this.Position = _potentialPosition - _rayToNearest.NormalizedCopy() * overlap;

                this.hasCollided = true;

                return true;
            }

            return false;
        }

        ~CircularColliderComponent() {}    
    }
}