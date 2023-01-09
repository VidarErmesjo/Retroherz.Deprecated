using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
	public partial class ColliderSystem
	{
		// Courtesy of One Lone Coder
        // https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_Rectangles.cpp
        
		//[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static bool Intersects(
			(Vector2 Position, Vector2 Direction) ray,
            (ColliderComponent collider, TransformComponent transform) rectangle,
            out Vector2 contactPoint,
            out Vector2 contactNormal,
            out float timeHitNear)
        {
            contactPoint = Vector2.Zero;
            contactNormal = Vector2.Zero;
            timeHitNear = 0f;

            // Are we in the correct (South-East) quadrant?
			// ... then calculate out-of-bands offset
        	(bool outOfBounds, Vector2 offset) state = (
				rectangle.transform.Position.X < 0 || rectangle.transform.Position.Y < 0,
				rectangle.collider.Size - ray.Position
			);

			state.outOfBounds = false;

            // To not confuse the algorithm on off-grid bounds we shift rectangle position and ray origin
            // to force upcomming calculations over to the positive axes.
            // - VE
            if (state.outOfBounds)
            {
                rectangle.transform.Position += state.offset;
                ray.Position += state.offset;
            }

             // Cache division
            var inverseDirection = Vector2.One / ray.Direction;

            // Calculate intersections with rectangle boundings axes
            var targetNear = (rectangle.transform.Position - ray.Position) * inverseDirection;
            var targetFar = (rectangle.transform.Position + rectangle.collider.Size - ray.Position) * inverseDirection;
          
            if (float.IsNaN(targetFar.X) || float.IsNaN(targetFar.Y)) return false;
            if (float.IsNaN(targetNear.X) || float.IsNaN(targetNear.Y)) return false;

            // Sort distances
            if (targetNear.X > targetFar.X) Utils.Swap(ref targetNear.X, ref targetFar.X);
            if (targetNear.Y > targetFar.Y) Utils.Swap(ref targetNear.Y, ref targetFar.Y);

            // Early rejection
            if (targetNear.X > targetFar.Y || targetNear.Y > targetFar.X) return false;

            // Closest 'time' will be the first contact
            timeHitNear = MathF.Max(targetNear.X, targetNear.Y);

            // Furthest 'time' is contact on opposite side of rectangle
            var timeHitFar = MathF.Min(targetFar.X, targetFar.Y);
            
            // Reject if ray directon is pointing away from object (can be usefull)
            if (timeHitFar < 0) return false;

			// For a correct calculation of contact point we shift ray origin back
            // - VE
            if (state.outOfBounds) ray.Position -= state.offset;

            // Contact point of collision from parametric line equation
            contactPoint = ray.Position + timeHitNear * ray.Direction;

            if (targetNear.X > targetNear.Y)
                contactNormal = inverseDirection.X < 0 ? Vector2.UnitX : -Vector2.UnitX;
            else if (targetNear.X < targetNear.Y)
                contactNormal = inverseDirection.Y < 0 ? Vector2.UnitY : -Vector2.UnitY;
            else
                // Note if targetNear == targetFar, collision is principly in a diagonal
                // so pointless to resolve. By returning a CN={0,0} even though its
                // considered a hit, the resolver wont change anything.

                // Diagonal case will be resolved in collision resolver. Thus we return CN={0,0}.
                // - VE
				contactNormal = Vector2.Zero;

            return true;
        }
    }
}