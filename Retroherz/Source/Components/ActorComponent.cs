using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;

namespace Retroherz.Components
{
    public enum ActorComponentClass
    {
        Player, // Wizard!!!
        NPC,
        Enemy
    }
    
    public class ActorComponent //: SimpleGameComponent
    {
        public float MaxSpeed { get; private set; }
		public bool Focused { get; private set; }

        public ActorComponent(float maxSpeed = 100.0f)
        {
			Focused = true;
            MaxSpeed = maxSpeed;
        }
        
        ~ActorComponent() {}
    }
}