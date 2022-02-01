using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;

namespace Retroherz.Components
{
    public enum PhysicsComponentType
    {
        Static,
        Dynamic
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
    public class PhysicsComponent
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Direction { get; set; }

        //public BoundingRectangle Size { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Origin { get; set; }
        public PhysicsComponent[] Contact = new PhysicsComponent[4];

        public PhysicsComponentType Type { get; set; }

        // EXP
        public Bag<Tuple<PhysicsComponent, Vector2, Vector2, float>> ContactInfo;
    
        // EXP

        public PhysicsComponent(PhysicsComponent physicsComponent)
        {
            Position = physicsComponent.Position;
            Velocity = physicsComponent.Velocity;
            Direction = physicsComponent.Direction;
            Size = physicsComponent.Size;
            Origin = physicsComponent.Origin;
            Contact = physicsComponent.Contact;
            ContactInfo = physicsComponent.ContactInfo;
            Type = physicsComponent.Type;
        }

        public PhysicsComponent(
            Vector2 position = default(Vector2),
            Vector2 velocity = default(Vector2),
            Vector2 direction = default(Vector2),
            Vector2 size = default(Vector2),
            PhysicsComponentType type = default(PhysicsComponentType))
        {
            Position = position;
            Velocity = velocity;
            Direction = direction;
            Size = size;
            Origin = size / 2;

            Contact[0] = null;
            Contact[1] = null;
            Contact[2] = null;
            Contact[3] = null;
            ContactInfo = new Bag<Tuple<PhysicsComponent, Vector2, Vector2, float>>();

            Type = type;
        }

        ~PhysicsComponent() {}
    }
}