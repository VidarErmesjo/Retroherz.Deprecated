using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace Retroherz.Components
{
    public class ColliderComponent : ICollisionActor
    {
        public enum ColliderComponentType
        {
            Dynamic,
            Static//, Wall
        }

        public ColliderComponentType Type { get; set; }

        public IShapeF Bounds { get; }

        public Vector2 PenetrationNormal { get; set; }

        public ColliderComponent[] Contact = new ColliderComponent[4];

        public ColliderComponent(IShapeF bounds, ColliderComponentType type = default(ColliderComponentType))
        {
            Bounds = bounds;
            Type = type;
            Contact[0] = null;
            Contact[1] = null;
            Contact[2] = null;
            Contact[3] = null;
        }

        ~ColliderComponent() {}

        public void OnCollision(CollisionEventArgs collisionEventArgs)
        {
            this.PenetrationNormal = Vector2.Zero;

            switch (this.Type)
            {
                case ColliderComponentType.Static:
                    return;
                case ColliderComponentType.Dynamic:
                {
                    var other = (ColliderComponent) collisionEventArgs.Other;
                    //other.Bounds.Position += collisionEventArgs.PenetrationVector;
                    this.Bounds.Position -= collisionEventArgs.PenetrationVector;
                    this.PenetrationNormal = collisionEventArgs.PenetrationVector.NormalizedCopy();
                    this.Contact[0] = PenetrationNormal.Y > 0 ? other : null;   // SOUTH
                    this.Contact[1] = PenetrationNormal.X < 0 ? other : null;   // WEST
                    this.Contact[2] = PenetrationNormal.Y < 0 ? other : null;   // NORTH
                    this.Contact[3] = PenetrationNormal.X > 0 ? other : null;   // EAST
                    break;
                }
                default:
                    break;
            }
        }

        // DrawRectangle()
        public static implicit operator RectangleF(ColliderComponent colliderComponent)
        {
            if(colliderComponent.Bounds.GetType() == typeof(CircleF))
                return RectangleF.Empty;

            var rectangle = (RectangleF) colliderComponent.Bounds;
            return new RectangleF(rectangle.Position, rectangle.Size);
        }
        
        // DrawCircle()
        public static implicit operator CircleF(ColliderComponent colliderComponent)
        {
            if(colliderComponent.Bounds.GetType() == typeof(RectangleF))
                return new CircleF(Point2.NaN, 0);

            var circle = (CircleF) colliderComponent.Bounds;
            return new CircleF(circle.Center, circle.Radius);
        }

        public static implicit operator Vector2(ColliderComponent colliderComponent)
        {
            var output = Vector2.Zero;
            var type = colliderComponent.Bounds.GetType();

            if (type == typeof(RectangleF))
            {
                var rectangle = (RectangleF) colliderComponent.Bounds;
                output = new Vector2(rectangle.Width, rectangle.Height);
            }
            else if (type == typeof(CircleF))
            {
                var circle = (CircleF) colliderComponent.Bounds;
                output = new Vector2(circle.Radius, circle.Radius);
            }

            return output;
        }

        public static implicit operator Point2(ColliderComponent colliderComponent)
        {
            var rectangle = (RectangleF) colliderComponent.Bounds;
            return new Point2(rectangle.Width, rectangle.Height);
        }

        public static implicit operator Size2(ColliderComponent colliderComponent)
        {
            var rectangle = (RectangleF) colliderComponent.Bounds;
            return new Size2(rectangle.Width, rectangle.Height);
        }

        /*public static implicit operator CollisionActor(ColliderComponent colliderComponent)
        {
            if (colliderComponent.Bounds.GetType() == typeof(RectangleF))
                colliderComponent.Bounds.Position -= Vector2.One * 8;

            return colliderComponent;
        }*/

        public override string ToString() => this.Bounds.ToString();
    }
}