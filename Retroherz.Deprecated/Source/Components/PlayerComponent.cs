using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended.Collections;

namespace Retroherz.Components
{
	// ControllerComponent ??
    public class PlayerComponent //: SimpleGameComponent
    {
		//public Dictionary<int, (ColliderComponent collider, TransformComponent transform)> Focused;
		//public Dictionary<int, (ColliderComponent collider, TransformComponent transform)> Selected;
		public Bag<int> Focused;
		public Bag<int> Selected;
        public float MaxSpeed { get; private set; } // Acceleration??

        public PlayerComponent(float maxSpeed = 100.0f)
        {
			Focused = new();
			Selected = new();
            MaxSpeed = maxSpeed;
        }
        
        ~PlayerComponent() {}
    }
}