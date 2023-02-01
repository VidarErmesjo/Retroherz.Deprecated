using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Shapes;
using MonoGame.Aseprite.Documents;
using MonoGame.Aseprite.Graphics;
using Retroherz.Collections;
using Retroherz.Math;

namespace Retroherz.Components
{
    public class SpriteComponent : SimpleGameComponent
    {
        private readonly AnimatedSprite _animatedSprite;
		private readonly Bag<Polygon> _slices;

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

		///	<summary>
		///	Aseprite "Slices".
		///	</summary>
		///	<returns>
		///	The bounds of the "Slices".
		///	</returns>
		public ReadOnlySpan<Polygon> Slices => _slices.AsSpan();

        public float Rotation
        {
            get => _animatedSprite.Rotation;
            set => _animatedSprite.Rotation = value;
        }

        /// <returns>
        /// The transform.
        /// </returns>
        public Matrix Transform =>  Matrix.Identity *
                                    Matrix.CreateTranslation(-this.Origin.X, -this.Origin.Y, 0) *
                                    Matrix.CreateScale(this.Scale.X) *
                                    Matrix.CreateRotationZ(this.Rotation) *
                                    Matrix.CreateTranslation(this.Position.X, this.Position.Y, 0);

		/// <summary>
		///	Construct sprite using <see cref="AsepriteDocument" />.
		/// </summary>
        public SpriteComponent(in AsepriteDocument asepriteDocument)
        {
            _animatedSprite = new AnimatedSprite(asepriteDocument);
			_slices = new Bag<Polygon>(_animatedSprite.Slices.Count);
        }

		///	<summary>
		///	Construct sprite using <see cref="Texture2D" />.
		///	</summary>
		public SpriteComponent(in Texture2D texture, in Vector position = default(Vector))
		{
			_animatedSprite = new AnimatedSprite(texture, position);
			_slices = new Bag<Polygon>(1);
		}

		///	<summary>
		///	Updates sprite animation.
		///	</summary>
        public override void Update(GameTime gameTime)
		{
			_animatedSprite.Update(gameTime);

			UpdateSlices();
		}
        
		///	<summary>
		///	Renders the sprite.
		///	</summary>
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
					layerDepth: 1
				);
		}

		///	<summary>
		///	Sets the current animation.
		///	</summary>
        public void Play(string animationName) => _animatedSprite.Play(animationName);

		private void UpdateSlices()
		{
			_slices.Clear();

			if (_animatedSprite.Animating)
				foreach (Slice slice in _animatedSprite.Slices.Values)
				{
					_slices.Add(new Polygon(new Vector2[]
						{
							Vector2.Transform(new Vector2(
								slice.Keys[_animatedSprite.CurrentFrameIndex].Bounds.Left,
								slice.Keys[_animatedSprite.CurrentFrameIndex].Bounds.Top), this.Transform),
							Vector2.Transform(new Vector2(
								slice.Keys[_animatedSprite.CurrentFrameIndex].Bounds.Right,
								slice.Keys[_animatedSprite.CurrentFrameIndex].Bounds.Top), this.Transform),
							Vector2.Transform(new Vector2(
								slice.Keys[_animatedSprite.CurrentFrameIndex].Bounds.Right,
								slice.Keys[_animatedSprite.CurrentFrameIndex].Bounds.Bottom), this.Transform),
							Vector2.Transform(new Vector2(
								slice.Keys[_animatedSprite.CurrentFrameIndex].Bounds.Left,
								slice.Keys[_animatedSprite.CurrentFrameIndex].Bounds.Bottom), this.Transform)
						}
					));
				}
			else
			{				
				_slices.Add(new Polygon(new Vector2[]
					{
						Vector2.Transform(new Vector2(
							_animatedSprite.Texture.Bounds.Left,
							_animatedSprite.Texture.Bounds.Top), this.Transform),
						Vector2.Transform(new Vector2(
							_animatedSprite.Texture.Bounds.Right,
							_animatedSprite.Texture.Bounds.Top), this.Transform),
						Vector2.Transform(new Vector2(
							_animatedSprite.Texture.Bounds.Right,
							_animatedSprite.Texture.Bounds.Bottom), this.Transform),
						Vector2.Transform(new Vector2(
							_animatedSprite.Texture.Bounds.Left,
							_animatedSprite.Texture.Bounds.Bottom), this.Transform)
					}
				));
			}
		}

        public override string ToString() => _animatedSprite.ToString();

		~SpriteComponent() {}
    }
}