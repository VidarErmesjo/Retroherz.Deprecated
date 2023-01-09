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

		// Needed?
        public Vector2 Origin
        {
            get => _animatedSprite.Origin;
            set => _animatedSprite.Origin = value;
        }

        public Vector2 Scale
        {
            get => _animatedSprite.Scale;
			set
			{
				if (_animatedSprite.Animating)
					_animatedSprite.Scale = new Vector2(
						value.X / _animatedSprite.Width,
						value.Y / _animatedSprite.Height);
				else
					_animatedSprite.Scale = new Vector2(
						value.X / _animatedSprite.Texture.Width,
						value.Y / _animatedSprite.Texture.Height);
			}
        }

        public float Rotation
        {
            get => _animatedSprite.Rotation;
            set => _animatedSprite.Rotation = value;
        }

        public SpriteComponent(AsepriteDocument asepriteDocument)
        {
            _animatedSprite = new AnimatedSprite(asepriteDocument);
        }

		public SpriteComponent(Texture2D texture, Vector2 position = default(Vector2))
		{
			_animatedSprite = new AnimatedSprite(texture, position);
		}

        public override void Update(GameTime gameTime) => _animatedSprite.Update(gameTime);
        
        public void Draw(SpriteBatch spriteBatch)
		{
			if (_animatedSprite.Animating)
				_animatedSprite.Render(spriteBatch);
			else
				spriteBatch.Draw(
					texture: _animatedSprite.Texture,
					position: this.Position,
					sourceRectangle: null,
					color: Color.White,
					rotation: 0,
					origin: this.Origin,
					scale: this.Scale,
					effects: SpriteEffects.None,
					layerDepth: 0);
		}

        public void Play(string animationName) => _animatedSprite.Play(animationName);

        public override string ToString() => _animatedSprite.ToString();

		~SpriteComponent() {}
    }
}