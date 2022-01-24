using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace Retroherz
{
    public class HUD : IDisposable
    {
        private bool isDisposed;

        private readonly Retroherz _game;
        private SpriteFont _spriteFont;

        private float _fps;
        private System.TimeSpan _totalGameTime;
        private MouseState _mouseState;

        private OrthographicCamera _camera;
        private ViewportAdapter _viewportAdapter;

        public HUD(Retroherz game)
        {
            _game = game;
            _spriteFont = null;
        }
        
        public void LoadContent()
        {
            _spriteFont = _game.AssetsManager.Font("Consolas");
        }

        public void Initialize()
        {
            _camera = _game.GameManager.Camera;
            _viewportAdapter = _game.GameManager.ViewportAdapter;
        }   

        public void Update(GameTime gameTime)
        {
            var deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            _mouseState = _game.GameManager.MouseState;

            _totalGameTime = gameTime.TotalGameTime;

            _fps = 1 / (float) deltaTime;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _viewportAdapter.GetScaleMatrix() * _game.GameManager.ScaleToDevice);

                spriteBatch.DrawString(
                    _spriteFont,
                    "FPS: " + _fps.ToString("0"),
                    Vector2.Zero, //new Vector2(0.0f, _spriteFont.LineSpacing * _game.GameManager.ScaleToDevice),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    _game.GameManager.ScaleToDevice,
                    SpriteEffects.None,
                    0);


            spriteBatch.End();
        }

        public void UnloadContent()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(isDisposed)
                return;

            if(disposing)
            {
                _game.Dispose();
                _viewportAdapter.Dispose(); 
            }

            isDisposed = true;
        }
   
        ~HUD()
        {
            this.Dispose(false);
        }
    }
}