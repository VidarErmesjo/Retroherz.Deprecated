using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;

namespace Retroherz.Components
{
	// Intersection?
	public struct Hit
	{
		public int Id;
		public float Distance;

		public Hit(int id, float distance)
		{
			Id = id;
			Distance = distance;
		}
	}

    public class RayComponent
    {
        public Vector2 Origin { get; }
        public Vector2 Direction { get; }

		public CircleF Circle => new(this.Center, this.Radius);
		public Point2 Center { get; set; }
		public float Radius { get; }
		public Bag<Hit> Hits;
		//public Dictionary<int, ((ColliderComponent collider, TransformComponent transform) target, Vector2 contactPoint, Vector2 contactNormal, float contactTime)> Hits;

		
        public RayComponent(RayComponent rayComponent)
        {
			Center = rayComponent.Center;
			Radius = rayComponent.Radius;
			Hits = rayComponent.Hits;
        }

        public RayComponent(Point2 center = default(Point2), float radius = 1)
        {
            //Origin = origin;
            //Direction = direction;
			Center = center;
			Radius = radius;
			Hits = new();
        }

        public static implicit operator CircleF(RayComponent rayComponent)
        {
            return new(rayComponent.Center, rayComponent.Radius);
        }

        ~RayComponent() {}        
    }
}