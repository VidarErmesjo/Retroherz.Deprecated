using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Aseprite.Documents;
using MonoGame.Aseprite.Graphics;

namespace Retroherz.Components
{
    public class SpriteComponent : SimpleGameComponent
    {
        private readonly AnimatedSprite _animatedSprite;

        public Vector2 Position
        {
            get => _animatedSprite.Position;
            set => _animatedSprite.Position = value;
        }

        public Vector2 Origin
        {
            get => _animatedSprite.Origin;
            set => _animatedSprite.Origin = value;
        }

        public Vector2 Scale
        {
            get => _animatedSprite.Scale;
            set => _animatedSprite.Scale = new Vector2(
                value.X / _animatedSprite.Width,
                value.Y / _animatedSprite.Height);
        }

        public float Rotation
        {
            get => _animatedSprite.Rotation;
            set => _animatedSprite.Rotation = value;
        }

        public SpriteComponent(ref AsepriteDocument asepriteDocument)
        {
            _animatedSprite = new AnimatedSprite(asepriteDocument);
        }

        ~SpriteComponent() {}

        public override void Update(GameTime gameTime) => _animatedSprite.Update(gameTime);
        
        public void Draw(SpriteBatch spriteBatch) => _animatedSprite.Render(spriteBatch);

        public void Play(string animationName) => _animatedSprite.Play(animationName);

        public override string ToString() => _animatedSprite.ToString();
    }
}