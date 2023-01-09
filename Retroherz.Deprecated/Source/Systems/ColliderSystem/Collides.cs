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
        internal static bool Collides(
            (ColliderComponent collider, TransformComponent transform) source,
            (ColliderComponent collider, TransformComponent transform) target,
            out Vector2 contactPoint,
            out Vector2 contactNormal,
            out float contactTime,
            float deltaTime)
        {
            contactPoint = Vector2.Zero;
            contactNormal = Vector2.Zero;
            contactTime = 0f;

			// Update positions if size has changed
			/*if (source.collider.DeltaSize != Vector2.Zero)
				source.transform.Position += -source.collider.DeltaOrigin;

			if (target.collider.DeltaSize != Vector2.Zero)
				target.transform.Position += -target.collider.DeltaOrigin;*/


			
			//var force = source.collider.DeltaSize * source.collider.Velocity.NormalizedCopy();
			//if (!force.IsNaN()) //force = Vector2.Zero;
				//System.Console.WriteLine($"Force:{force}");


			// Calculate ray vectors
			(Vector2 Position, Vector2 Direction) ray = new(
            	source.transform.Position + source.collider.Origin,
            	//(source.collider.Velocity + 1 * source.collider.DeltaSize * source.collider.Velocity.NormalizedCopy()) * deltaTime);
				source.collider.Velocity * deltaTime);

			// Check if source is changing size and nudge it
			/*if (source.collider.DeltaSize != Vector2.Zero)
			{
				// Inert and changing size
				var sourceRectangle = Utils.ProspectiveRectangle(source.collider, source.transform, deltaTime);
				//var sourceRectangle = new RectangleF(source.transform.Position + source.collider.Velocity * deltaTime, source.collider.Size);
				var targetRectangle = new RectangleF(target.transform.Position, target.collider.Size);
				var intersectingRectangle = sourceRectangle.Intersection(targetRectangle);

                // Early rejection
                if (intersectingRectangle.IsEmpty) return false;

                Vector2 penetration;
                if (intersectingRectangle.Width < intersectingRectangle.Height)
                {
                    var displacement = sourceRectangle.Center.X < targetRectangle.Center.X
                        ? intersectingRectangle.Width
                        : -intersectingRectangle.Width;
                    penetration = Vector2.UnitX * displacement;
                }
                else
                {
                    var displacement = sourceRectangle.Center.Y < targetRectangle.Center.Y
                        ? intersectingRectangle.Height
                        : -intersectingRectangle.Height;
                    penetration = Vector2.UnitY * displacement;
                }

				//Vector2.Clamp(penetration, -Vector2.One, Vector2.One);
				// Nudge position
				source.transform.Position += -penetration;
				source.collider.Velocity -= source.collider.Velocity + penetration;


				if (penetration.X >= 1f || penetration.Y >= 1f)
				{
					//source.collider.Size -= source.collider.DeltaSize;
					//source.collider.Velocity = Vector2.Zero;
									// Nudge
					//source.collider.Velocity += -penetration;
				}
				else
				{

					// Shift position
					//source.transform.Position += -penetration;
				}
				//System.Console.WriteLine($"Penetration:{penetration} Velocity:{source.collider.Velocity}");

				//contactNormal = -penetration.NormalizedCopy();
                //if (float.IsNaN(contactNormal.X) || float.IsNaN(contactNormal.Y)) contactNormal = Vector2.Zero;

				//return (contactNormal != Vector2.Zero) ? true : false;


				// Nudge
				//source.collider.Velocity += -penetration;
				//rayDirection = -penetration * deltaTime;

				//System.Console.WriteLine($"Penetration:{penetration}");
				//System.Console.WriteLine($"Trangt! {penetration.X > 1f || penetration.Y > 1f}");
			}*/
               
            // Check if source is actually moving
            if (source.collider.Velocity == Vector2.Zero) return false;
           
            // Expand target collider box by source dimensions
            (ColliderComponent collider, TransformComponent transform) rectangle = (
                new ColliderComponent(
                    size: target.collider.Size + source.collider.Size,
                    velocity: target.collider.Velocity,
                    type: target.collider.Type),
                new TransformComponent(position: target.transform.Position - source.collider.Origin));

			// Create delta size
			//rectangle.collider.Size += source.collider.Size;
			
            // EXP
            /*if (target.collider.Type == ColliderComponentType.Dynamic)
            {
				System.Console.WriteLine("{0}", target.collider.Type.ToString());

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
                    penetration = Vector2.UnitX * displacement;
                }
                else
                {
                    var displacement = sourceRectangle.Center.Y < targetRectangle.Center.Y
                        ? intersectingRectangle.Height
                        : -intersectingRectangle.Height;
                    penetration = Vector2.UnitY * displacement;
                }

				//source.transform.Position += -penetration;

                contactNormal = -penetration.NormalizedCopy();
                if (float.IsNaN(contactNormal.X) || float.IsNaN(contactNormal.Y)) contactNormal = Vector2.Zero;

                contactPoint = source.transform.Position + source.collider.Origin;
                contactTime = 0.0f;// = (penetration / target.collider.Size).Length();

                // FUN
                //target.collider.Velocity += -contactNormal * new Vector2(MathF.Abs(source.collider.Velocity.X), MathF.Abs(source.collider.Velocity.Y) * contactTime);
                //source.collider.Velocity += contactNormal * new Vector2(MathF.Abs(target.collider.Velocity.X), MathF.Abs(target.collider.Velocity.Y) * contactTime);

                return (contactNormal != Vector2.Zero) ? true : false;
            }*/

			//System.Console.WriteLine($"s.Size:{source.collider.Size} t.Size:{rectangle.collider.Size} diff:{rectangle.collider.Size - source.collider.Size}");
			//System.Console.WriteLine($"s.Pos:{source.transform.Position} t.Pos:{rectangle.transform.Position} diff:{rectangle.transform.Position - source.transform.Position}");

            // Cast ray
            if (Intersects(
				ray,
                rectangle,
                out contactPoint,
                out contactNormal,
                out contactTime))
                    return (contactTime >= 0f && contactTime < 1f);
            else 
                return false;
        }
    }
}