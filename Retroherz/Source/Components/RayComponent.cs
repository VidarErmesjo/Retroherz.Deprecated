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
		public (Vector2 Position, double Angle, double Distance)[] Edges = new (Vector2, double, double)[3];

		public Hit(int id, (Vector2, double, double)[] edges)
		{
			Id = id;
			Edges = edges;
		}
	}

    public class RayComponent
    {
        public Vector2 Origin { get; }
        public Vector2 Direction { get; }

		public CircleF Circle => new(this.Center, this.Radius);
		public Vector2 Center { get; set; }
		public float Radius { get; }
		public Bag<Hit> Hits = new();
		//public Dictionary<int, ((ColliderComponent collider, TransformComponent transform) target, Vector2 contactPoint, Vector2 contactNormal, float contactTime)> Hits;

		
        public RayComponent(RayComponent rayComponent)
        {
			Center = rayComponent.Center;
			Radius = rayComponent.Radius;
			Hits = rayComponent.Hits;
        }

        public RayComponent(Vector2 center = default(Vector2), float radius = 1)
        {
            //Origin = origin;
            //Direction = direction;
			Center = center;
			Radius = radius;
			//Hits = new();
        }

        public static implicit operator CircleF(RayComponent rayComponent)
        {
            return new(rayComponent.Center, rayComponent.Radius);
        }

        ~RayComponent() {}        
    }
}