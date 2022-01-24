using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;

namespace Retroherz.Components
{
    public enum PlayerComponentClass
    {
        Player, // Wizard!!!
        NPC,
        Enemy
    }
    
    public class PlayerComponent //: SimpleGameComponent
    {
        // EXPERIMENTAL
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Direction { get; set; }

        ////
        public Vector2 Origin { get; set; }
        public Vector2 Size { get; set; }

        public AnimatedSprite Sprite { get; set; }
        //

        public float MaxSpeed { get; private set; } // Acceleration??

        public PlayerComponent(float maxSpeed = 100.0f)
        {
            MaxSpeed = maxSpeed;
        }
        
        ~PlayerComponent() {}
    }
}