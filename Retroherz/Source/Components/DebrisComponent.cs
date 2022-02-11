using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Aseprite.Graphics;

namespace Retroherz.Components
{
    public class DebrisComponent
    {
        public Vector2 Velocity { get; set; }
        //public AnimatedSprite Sprite { get; set; }

        public DebrisComponent()
        {

        }

        ~DebrisComponent() {}        
    }
}