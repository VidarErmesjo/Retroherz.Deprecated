using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;

namespace Retroherz.Components
{
    // ActorComponent?
    public class PhysicsComponent
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        //public BoundingRectangle Size { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Origin { get; set; }
        public PhysicsComponent[] Contact = new PhysicsComponent[4];

        public ColliderComponentType Type { get; set; }

        // EXP
        public Bag<Tuple<PhysicsComponent, Vector2, Vector2, float>> ContactInfo;
    
        // EXP

        public PhysicsComponent(PhysicsComponent physicsComponent)
        {
            Position = physicsComponent.Position;
            Velocity = physicsComponent.Velocity;
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
            ColliderComponentType type = default(ColliderComponentType))
        {
            Position = position;
            Velocity = velocity;
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