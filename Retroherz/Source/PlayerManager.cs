using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Retroherz.Input
{
    interface IPlayerManager : IDisposable
    {
        void Update(GameTime gameTime);
    }

    public class PlayerManager : IPlayerManager
    {  
        private bool isDisposed;

        private readonly Retroherz _game;

        public PlayerManager(Retroherz game)
        {
            _game = game;
        }

        public void Update(GameTime gameTime) //?
        {
            var direction = new Vector2(
                _game.GameManager.KeyboardState.IsKeyDown(Keys.Left) ? -1.0f :
                _game.GameManager.KeyboardState.IsKeyDown(Keys.Right) ? 1.0f : 0.0f,  
                _game.GameManager.KeyboardState.IsKeyDown(Keys.Up) ? -1.0f:
                _game.GameManager.KeyboardState.IsKeyDown(Keys.Down) ? 1.0f : 0.0f);

            var angle = _game.GameManager.Camera.ScreenToWorld(
                new Vector2(_game.GameManager.MouseState.X, _game.GameManager.MouseState.Y));
            /*angle -= _game.EntityManager.Players.First().Position;

            _game.EntityManager.Players.First().Velocity = !direction.NormalizedCopy().IsNaN() ? direction : Vector2.Zero;
            _game.EntityManager.Players.First().Rotation = angle.ToAngle() + MathF.PI;*/
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
            }

            isDisposed = true;
        }

        ~PlayerManager()
        {
            this.Dispose(false);
        }        
    }
}