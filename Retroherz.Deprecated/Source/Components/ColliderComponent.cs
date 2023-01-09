using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;

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

	public struct Contact
	{
		public int Id;
		public Vector2 ContactPoint;
		public Vector2 ContactNormal;
		public float ContactTime;

		public Contact(int id, Vector2 contactPoint, Vector2 contactNormal, float contactTime)
		{
			Id = id;
			ContactPoint = contactPoint;
			ContactNormal = contactNormal;
			ContactTime = contactTime;
		}
	}

    public class ColliderComponent
    {
		private readonly Bag<Contact> _contacts = new();
		private Vector2 _size = default(Vector2);
		private Vector2 _deltaSize = default(Vector2);

        public Vector2 Velocity { get; set; }
        public Vector2 Size
		{
			get => _size;
			set
			{
				_deltaSize = _size;
				_size = Vector2.Clamp(value, Vector2.One, value);
			}
		}

		public Vector2 DeltaSize
		{
			get => _size - _deltaSize;
			private set => _deltaSize = Vector2.Clamp(value, Vector2.One, value);
		}

		public Vector2 Origin { get => Size / 2; }
		public Vector2 DeltaOrigin { get => DeltaSize / 2; }

        public ColliderComponentType Type { get; set; }

		public void Add(Contact contact) => _contacts.Add(contact);
		public void Clear() => _contacts.Clear();

		public Span<Contact> Contacts
		{
			get
			{
				// vs new Span<Contact>() ??
				var items = _contacts.ToArray().AsSpan();
				items.Sort((a, b) => a.ContactTime < b.ContactTime ? -1 : 1);
				return items;
			}
		}

        public ColliderComponent(
            Vector2 velocity = default(Vector2),
            Vector2 size = default(Vector2),
            ColliderComponentType type = default(ColliderComponentType))
        {
            Velocity = velocity;
            Size = size;
			DeltaSize = size;
            Type = type;
        }

		~ColliderComponent() {}
    }
}