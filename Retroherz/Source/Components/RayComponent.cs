using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;

using Retroherz.Math;

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
        public Vector Origin { get; }
        public Vector Direction { get; }

		public CircleF Circle => new(this.Center, this.Radius);
		public Vector Center { get; set; }
		public float Radius { get; }
		public Bag<Hit> Hits { get; }
		//public Dictionary<int, ((ColliderComponent collider, TransformComponent transform) target, Vector contactPoint, Vector contactNormal, float contactTime)> Hits;

		
        public RayComponent(RayComponent rayComponent)
        {
			Center = rayComponent.Center;
			Radius = rayComponent.Radius;
			Hits = rayComponent.Hits;
        }

        public RayComponent(Vector center = default(Vector), float radius = 1)
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