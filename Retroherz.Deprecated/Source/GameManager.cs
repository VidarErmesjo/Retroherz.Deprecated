using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace Retroherz
{
    public class GameManager : IDisposable
    {
        private bool isDisposed;

        private bool _setup = false;
        private bool _initialized = false;

        public GraphicsDeviceManager GraphicsDeviceManager;// { get; private set; }
        public GameWindow Window { get; private set; }
        public ContentManager Content { get; private set; }

        public static Size VirtualResolution { get; private set; }
        public static Size DeviceResolution { get; private set; }
        public RenderTarget2D DeviceRenderTarget { get; private set; }
        public RenderTarget2D VirtualRenderTarget { get; private set; }
        public Rectangle DeviceRectangle { get; private set; }
        public float ScaleToDevice { get; private set; }
        public bool IsFullScreen { get; private set; }
        public bool IsLowResolution { get; private set; }

        public OrthographicCamera Camera { get; private set; }
        public BoxingViewportAdapter ViewportAdapter { get; private set; }

        public MouseState MouseState { get; private set; }
        public MouseState PreviousMouseState { get; private set; }
        public KeyboardState KeyboardState { get; private set; }
        public KeyboardState PreviousKeyboardState { get; private set; }
        public GamePadState GamePadState { get; private set; }
        public GamePadState PreviousGamePadState { get; private set; }

        private const float _meterToSpriteRatio = 1f;
        public float SpriteSize { get; private set; }
        public float SpriteScale { get; private set; }
        public float MetersPerPixel { get; private set; }
        public Size2 MetersPerScreen { get; private set; }

        // TileWorld

        public const int UnitSize = 16; // 1U
        public const int TileWidth = UnitSize;
        public const int TileHeight = UnitSize; 
        public int VisibleTilesX { get; private set; }
        public int VisibleTilesY { get; private set; }

        public GameManager(Game game, Size resolution = default(Size), bool isFullscreen = true)
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(game);
            game.IsMouseVisible = true;
            game.IsFixedTimeStep = true;
            game.Content.RootDirectory = "Content";

            Content = game.Content;
            Window = game.Window;

            if(resolution == default(Size))
                resolution = new Size(3840, 2160);

            VirtualResolution = resolution;
            IsFullScreen = isFullscreen;

            _setup = true;
         }

        public void Initialize()
        {
            // Depricate?
            if(!_setup)
                throw new InvalidOperationException("Core.Setup() must preceed Core.Initialize()");

            if(_initialized)
                return;

            Window.AllowUserResizing = false;
            //Window.ClientSizeChanged() += OnChange();

            if(IsFullScreen)
                DeviceResolution = new Size(
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            else
                DeviceResolution = new Size(
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2,
                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2);

            DeviceRectangle = new Rectangle(0, 0, DeviceResolution.Width, DeviceResolution.Height);

            GraphicsDeviceManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDeviceManager.PreferredBackBufferWidth = DeviceResolution.Width;
            GraphicsDeviceManager.PreferredBackBufferHeight = DeviceResolution.Height;
            GraphicsDeviceManager.IsFullScreen = IsFullScreen;
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            GraphicsDeviceManager.HardwareModeSwitch = false;
            GraphicsDeviceManager.ApplyChanges();
            GraphicsDeviceManager.HardwareModeSwitch = true;            

            DeviceRenderTarget = new RenderTarget2D(
                graphicsDevice: GraphicsDeviceManager.GraphicsDevice,
                width: DeviceResolution.Width,
                height: DeviceResolution.Height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                preferredMultiSampleCount: GraphicsDeviceManager.GraphicsDevice.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.DiscardContents);


            VirtualRenderTarget = new RenderTarget2D(
                graphicsDevice: GraphicsDeviceManager.GraphicsDevice,
                width: VirtualResolution.Width,
                height: VirtualResolution.Height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                preferredMultiSampleCount: GraphicsDeviceManager.GraphicsDevice.PresentationParameters.MultiSampleCount,
                RenderTargetUsage.DiscardContents);

            ViewportAdapter = new BoxingViewportAdapter(
                Window,
                GraphicsDeviceManager.GraphicsDevice,
                VirtualResolution.Width,
                VirtualResolution.Height);
            Camera = new OrthographicCamera(ViewportAdapter);

            var scaleX = VirtualResolution.Width / (float) DeviceResolution.Width;
            var scaleY = VirtualResolution.Height / (float) DeviceResolution.Height;
            ScaleToDevice = MathF.Sqrt(scaleX * scaleX + scaleY * scaleY);

            MouseState = new MouseState();
            PreviousMouseState = new MouseState();
            Mouse.SetCursor(MouseCursor.Crosshair);
            KeyboardState = new KeyboardState();
            PreviousKeyboardState = new KeyboardState();
            GamePadState = new GamePadState();
            PreviousGamePadState = new GamePadState();

            SpriteSize = 16 * _meterToSpriteRatio;
            SpriteScale = 1f;
            MetersPerPixel = 1 / (SpriteSize * SpriteScale);
            MetersPerScreen = new Size2(
                VirtualResolution.Width,
                VirtualResolution.Height) * MetersPerPixel;

            // TileWorld

            VisibleTilesX = VirtualResolution.Width / (int) TileWidth;
            VisibleTilesY = VirtualResolution.Height / (int) TileHeight;
            System.Console.WriteLine("VisibleTilesX: {0}, VisibleTilesY: {1}", VisibleTilesX, VisibleTilesY);

            // Done!
            _initialized = true;

            System.Console.WriteLine("Core.Initialize() => OK");
            System.Console.WriteLine("VirtualResolution => {0}, DeviceResolution => {1}, ScaleToDevice => {2}", VirtualResolution, DeviceResolution, ScaleToDevice);
        }

        public void Update()
        {
            PreviousMouseState = MouseState;
            PreviousKeyboardState = KeyboardState;
            PreviousGamePadState = GamePadState;
            MouseState = Mouse.GetState();
            KeyboardState = Keyboard.GetState();
            GamePadState = GamePad.GetState(PlayerIndex.One);
        }

        public void UnloadContent()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(isDisposed)
                return;

            if(disposing)
            {
                GraphicsDeviceManager.Dispose();
                DeviceRenderTarget.Dispose();
                VirtualRenderTarget.Dispose();
                ViewportAdapter.Dispose();
                System.Console.WriteLine("Core.Dispose() => OK");
            }

            isDisposed = true;
        }

        ~GameManager()
        {
            Dispose(false);
        }

    }
}