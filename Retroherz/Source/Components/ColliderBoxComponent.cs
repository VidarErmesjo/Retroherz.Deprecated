using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

namespace Retroherz.Components
{
    public class RectangularColliderComponent
    {
        public bool hasCollided { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public readonly Vector2 Size;
        public RectangularColliderComponent[] Contact = new RectangularColliderComponent[4];

        public RectangularColliderComponent(
            Vector2 position = default(Vector2),
            Vector2 velocity = default(Vector2),
            Vector2 size = default(Vector2))
        {
            Position = position;
            Velocity = velocity;
            Size = size;
            Contact[0] = null;
            Contact[1] = null;
            Contact[2] = null;
            Contact[3] = null;
            hasCollided = false;
        }

        public static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

        public bool Ray(
            ref RectangularColliderComponent target,
            float deltaTime,
            ref Vector2 contactPoint,
            ref Vector2 contactNormal,
            ref float timeHitNear)
        {
            // Calculate ray vectors
            Vector2 rayOrigin = this.Position + this.Size / 2;
            Vector2 rayDirection = this.Velocity * deltaTime;
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
            ref RectangularColliderComponent obstacle,
            float deltaTime,
            ref Vector2 contactPoint,
            ref Vector2 contactNormal,
            ref float contactTime)
        {
            // Check if rectangle is actually moving - we assume rectangles are NOT in collision on start
            if (this.Velocity.X == 0 && this.Velocity.Y == 0) return false;

            // Expand target collider box by source dimensions
            var inflated = new RectangularColliderComponent(
                obstacle.Position - this.Size / 2,
                this.Velocity,
                obstacle.Size + this.Size);
            
            if (Ray(
                ref inflated,
                deltaTime,
                ref contactPoint,
                ref contactNormal,
                ref contactTime))
                    return (contactTime >= 0.0f && contactTime < 1.0f); // Clamp?
            else 
                return false;
        }

        public bool Resolve(
            RectangularColliderComponent obstacle,
            float deltaTime)
        {
            var contactPoint = new Vector2();
            var contactNormal = new Vector2();

            float contactTime = 0.0f;

            if (Intersects(
                ref obstacle,
                deltaTime,
                ref contactPoint,
                ref contactNormal,
                ref contactTime))
            {
                this.Contact[0] = contactNormal.Y > 0 ? obstacle : null;
                this.Contact[1] = contactNormal.X < 0 ? obstacle : null;
                this.Contact[2] = contactNormal.Y < 0 ? obstacle : null;
                this.Contact[3] = contactNormal.X > 0 ? obstacle : null;

                this.Velocity += contactNormal * new Vector2(
                    MathF.Abs(this.Velocity.X),
                    MathF.Abs(this.Velocity.Y)) * (1 - contactTime);

                this.hasCollided = true;

                return true;
            }
            return false;
        }

        public static RectangularColliderComponent ToRectangularColliderComponent(TiledMapTile tile, TiledMap tileMap)
        {
            return new RectangularColliderComponent(
                new Vector2(tile.X * tileMap.TileWidth, tile.Y * tileMap.TileHeight),
                Vector2.Zero,
                new Size2(tileMap.TileWidth, tileMap.TileHeight));
        }

        ~RectangularColliderComponent() {}
    }
}