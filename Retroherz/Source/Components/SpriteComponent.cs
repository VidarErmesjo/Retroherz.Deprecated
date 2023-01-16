using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Aseprite.Documents;
using MonoGame.Aseprite.Graphics;
using Retroherz.Math;

namespace Retroherz.Components
{
    public class SpriteComponent : SimpleGameComponent
    {
        private readonly AnimatedSprite _animatedSprite;

        public Vector Position
        {
            get => _animatedSprite.Position;
            set => _animatedSprite.Position = value;
        }

		// Needed?
        public Vector Origin
        {
            get => _animatedSprite.Origin;
            set => _animatedSprite.Origin = value;
        }

        public Vector Scale
        {
            get => _animatedSprite.Scale;
			set
			{
				if (_animatedSprite.Animating)
					_animatedSprite.Scale = new Vector(
						value.X / _animatedSprite.Width,
						value.Y / _animatedSprite.Height);
				else
					_animatedSprite.Scale = new Vector(
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

		public SpriteComponent(Texture2D texture, Vector position = default(Vector))
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
					layerDepth: 1);
		}

        public void Play(string animationName) => _animatedSprite.Play(animationName);

        public override string ToString() => _animatedSprite.ToString();

		~SpriteComponent() {}
    }
}