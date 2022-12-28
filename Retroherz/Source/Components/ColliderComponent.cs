using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Collisions;

using Retroherz.Systems;

namespace Retroherz.Components
{
    public enum ColliderComponentType
    {
        Static,
        Dynamic,
        Border
    }

    public enum ResolveCollision
    {
        Stop,
        Slide,
        Bounce
    }

    public enum Side
    {
        North,
        East,
        South,
        West
    }

    // ActorComponent?
    public class ColliderComponent : ICollisionActor
    {
        public Vector2 Velocity { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Origin { get; set; }
        public ColliderComponentType Type { get; set; }

        public Nullable<(ColliderComponent, TransformComponent)>[] Contact;

        public IShapeF Bounds { get; }
        public Vector2 PenetrationVector { get; set; }

        // EXP
        public Bag<((ColliderComponent collider, TransformComponent tranform) target, Vector2 contactPoint, Vector2 contactNormal, float contactTime)> ContactInfo;

        public Bag<Vector2> PenetrationVectors;

        public ColliderComponent(ColliderComponent colliderComponent)
        {
            Velocity = colliderComponent.Velocity;
            Size = colliderComponent.Size;
            Origin = colliderComponent.Origin;
            Contact = colliderComponent.Contact;
            ContactInfo = colliderComponent.ContactInfo;
            Type = colliderComponent.Type;
        }

        public ColliderComponent(
            Vector2 velocity = default(Vector2),
            Vector2 size = default(Vector2),
            ColliderComponentType type = default(ColliderComponentType))
        {
            Velocity = velocity;
            Size = size;
            Origin = size / 2;

            Contact = new Nullable<(ColliderComponent, TransformComponent)>[4] { null, null , null, null };

            ContactInfo = new Bag<((ColliderComponent, TransformComponent), Vector2, Vector2, float)>();

            Type = type;

            PenetrationVector = Vector2.Zero;
        }

        /*public ColliderComponent(
            IShapeF bounds,
            Vector2 velocity = default(Vector2),
            ColliderComponentType type = default(ColliderComponentType))
        {
            Bounds = bounds;
            Velocity = velocity;
            Type = type;

            var size = (RectangleF) bounds;
            Size = new Vector2(size.Width, size.Height);
            Origin = Size / 2;

            Contact = new Nullable<(ColliderComponent, TransformComponent)>[4] { null, null , null, null };
            ContactInfo = new Bag<((ColliderComponent, TransformComponent), Vector2, Vector2, float)>();
            PenetrationVectors = new Bag<Vector2>(5);

        }*/

        public void OnCollision(CollisionEventArgs collisionEventArgs)
        {
            System.Console.WriteLine("{0} => {1}", Type, collisionEventArgs.PenetrationVector.NormalizedCopy());
            var other = (ColliderComponent) collisionEventArgs.Other;
            var transform = new TransformComponent(position: other.Bounds.Position);

            switch (Type)
            {
                case ColliderComponentType.Static:
                    break;
                case ColliderComponentType.Dynamic:
                    //Bounds.Position -= collisionEventArgs.PenetrationVector;

                    Contact[0] = collisionEventArgs.PenetrationVector.Y > 0 ? (other, transform) : null;
                    Contact[1] = collisionEventArgs.PenetrationVector.X < 0 ? (other, transform) : null;
                    Contact[2] = collisionEventArgs.PenetrationVector.Y < 0 ? (other, transform) : null;
                    Contact[3] = collisionEventArgs.PenetrationVector.X > 0 ? (other, transform) : null;
                    ContactInfo.Add(((other, transform), Vector2.Zero, Vector2.Zero, 0.0f));
                    PenetrationVectors.Add(collisionEventArgs.PenetrationVector);

                    /*if(collisionEventArgs.PenetrationVector.X < 0 || collisionEventArgs.PenetrationVector.X > 0)
                        Velocity = Velocity.SetX(0);
                    if(collisionEventArgs.PenetrationVector.Y < 0 || collisionEventArgs.PenetrationVector.Y > 0)
                        Velocity = Velocity.SetY(0);*/

                    if (collisionEventArgs.PenetrationVector.X < 0)
                        if (Velocity.X < 0) Velocity = Velocity.SetX(0);
                    if (collisionEventArgs.PenetrationVector.X > 0)
                        if (Velocity.X > 0) Velocity = Velocity.SetX(0);
                    if (collisionEventArgs.PenetrationVector.Y < 0)
                        if (Velocity.Y < 0) Velocity = Velocity.SetY(0);
                    if (collisionEventArgs.PenetrationVector.Y > 0)
                        if (Velocity.Y > 0) Velocity = Velocity.SetY(0);

                    if (other.Type == ColliderComponentType.Static);
                        //System.Console.WriteLine("STATIC");
                    else if (other.Type == ColliderComponentType.Dynamic)
                    {
                        //other.Velocity += collisionEventArgs.PenetrationVector * 32;
                        other.Velocity *= -1;
                        //System.Console.WriteLine("DYNAMIC");
                    }
                    else if (other.Type == ColliderComponentType.Border);
                        //System.Console.WriteLine("BORDER");

                    break;
                default:
                    break;
            }
        }

        // ToString() Position: {X:180.2995 Y:64}, Rotation: 0, Scale: {X:0 Y:0}, Retroherz.Components.ColliderComponent

        ~ColliderComponent() {}
    }
}