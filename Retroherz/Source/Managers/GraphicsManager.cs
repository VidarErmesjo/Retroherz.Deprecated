using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.ViewportAdapters;

namespace Retroherz.Managers;

public enum GraphicsMode
{
	Default = 0,
	OneEightyP,
	ThreeSixtyP,
	FourFiftyP,
	SevenTwentyP,
	ThousandEightyP,
	FourK,
	EightK,
	NES,
	SNES,
	Genesis
}

/// <summary>
///	Sets up graphics and display.
/// </summary>
public class GraphicsManager : SimpleGameComponent, IDisposable
{
	private bool _isDisposed;

	private OrthographicCamera _camera;
	private readonly Dictionary<string, Effect> _effects = new();
	private readonly GraphicsDeviceManager _graphicsDeviceManager;
	private bool _isFullScreen;
	private RenderTarget2D _renderTarget;
	private readonly Size _renderTargetResolution;
	private readonly Dictionary<string, RenderTarget2D> _renderTargets = new();
	private SpriteBatch _spriteBatch;
	private readonly Size _virtualResolution;
	private BoxingViewportAdapter _viewportAdapter;
	public Size _viewportResolution;
	private readonly GameWindow _window;

	/// <summary>
	///	Returns the state of full screen.
	/// </summary>
	public bool IsFullScreen => _graphicsDeviceManager.IsFullScreen;

	/// <summary>
	///	Returns the device rectangle.
	/// </summary>
	public Rectangle DestinationRectangle { get; private set; }

	/// <summary>
	///	Returns the native render target.
	/// </summary>
	public RenderTarget2D RenderTarget => _renderTarget;

	/// <summary>
	///	Returns the virtual (rendering) resolition.
	/// </summary>
	public Size VirtualResolution => _virtualResolution;

	/// <summary>
	///	Returns the viewport (device) resolution.
	/// </summary>
	public Size ViewportResolution => _viewportResolution;

	// TODO: Fix scaling! Should be "boxing".
	public GraphicsManager(Game game, GraphicsMode displayMode, bool isFullScreen = true, bool scale = false)
	{
		// GraphicsManager
		game.IsFixedTimeStep = true;
        game.IsMouseVisible = true;
		game.Window.AllowUserResizing = true; 

		// Create graphics context
		_graphicsDeviceManager = new GraphicsDeviceManager(game);
		_window = game.Window;

		// Calculate the viewport (internal) resolution
		var displayModeInfo = DisplayModes[((ushort)displayMode)];

		_renderTargetResolution = new Size(
			((int)displayModeInfo.Resolution.Width),
			((int)displayModeInfo.Resolution.Height)
		);

		_virtualResolution = new Size(
			((int)(displayModeInfo.Resolution.Width / (scale ? displayModeInfo.ScaleFactor.Width : 1))),
			((int)(displayModeInfo.Resolution.Height / (scale ? displayModeInfo.ScaleFactor.Height : 1)))
		);

		_isFullScreen = isFullScreen;
	}

	public void SetIsFullScreen(bool isFullScreen) => this._isFullScreen = isFullScreen;

	/// <summary>
	///	Initializes the game manager and outputs the camera and the graphics device.
	/// </summary>
	public void Initialize(out OrthographicCamera camera, out GraphicsDevice graphics)
	{
		graphics = Initialize();
		camera = _camera;
	}

	/// <summary>
	///	Initializes the game manager and outputs the graphics device.
	/// </summary>
	public void Initialize(out GraphicsDevice graphics)
	{
		graphics = Initialize();
	}

	/// <summary>
	///	Initializes the game manager and returns the graphics device.
	/// </summary>
	public new GraphicsDevice Initialize()
	{
		//this.Game.Window.ClientSizeChanged() += OnChange();

		if (_isFullScreen)
			_viewportResolution = new Size(
				((ushort)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width),
				((ushort)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
			);
		else
			_viewportResolution = new Size(
				((ushort)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2)),
				((ushort)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2))
			);

		_graphicsDeviceManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
		_graphicsDeviceManager.PreferredBackBufferWidth = _viewportResolution.Width;
		_graphicsDeviceManager.PreferredBackBufferHeight = _viewportResolution.Height;
		_graphicsDeviceManager.IsFullScreen = _isFullScreen;
		_graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
		_graphicsDeviceManager.HardwareModeSwitch = false;
		_graphicsDeviceManager.ApplyChanges();
		_graphicsDeviceManager.HardwareModeSwitch = true;

		_spriteBatch = new(_graphicsDeviceManager.GraphicsDevice);

		this.DestinationRectangle = new Rectangle(
			0,
			0,
			_viewportResolution.Width,
			_viewportResolution.Height
		);

		// EXP: Render to viewport scale
		_renderTarget = new RenderTarget2D(
			_graphicsDeviceManager.GraphicsDevice,
			_renderTargetResolution.Width,//_viewportResolution.Width,//_virtualResolution.Width,
			_renderTargetResolution.Height,//_viewportResolution.Height,//_virtualResolution.Height,
			false,
			SurfaceFormat.Color,
			DepthFormat.None,
			_graphicsDeviceManager.GraphicsDevice.PresentationParameters.MultiSampleCount,
			RenderTargetUsage.DiscardContents
		);

		_viewportAdapter = new BoxingViewportAdapter(
			_window,
			_graphicsDeviceManager.GraphicsDevice,
			_virtualResolution.Width,
			_virtualResolution.Height
		);
	
		_camera = new(_viewportAdapter);

		return _graphicsDeviceManager.GraphicsDevice;
	}

	public override void Update(GameTime gameTime)
	{
		// if (_isFullScreen) ... handle change?

	}

	public RenderTarget2D CreateRenderTarget(
		string name,
		bool mipMap,
		SurfaceFormat preferredFormat,
		DepthFormat preferredDepthFormat,
		int preferredMultiSampleCount,
		RenderTargetUsage usage
	)
	{
		_renderTargets.Add(name.ToLower(), new RenderTarget2D(
				graphicsDevice: _graphicsDeviceManager.GraphicsDevice,
				width: _renderTargetResolution.Width,//_virtualResolution.Width,
				height: _renderTargetResolution.Height,//_virtualResolution.Height,
				mipMap: mipMap,
				preferredFormat: preferredFormat,
				preferredDepthFormat: preferredDepthFormat,
				preferredMultiSampleCount: preferredMultiSampleCount,
				RenderTargetUsage.DiscardContents
			)
		);

		return _renderTargets[name.ToLower()];
	}

	private const string INIT_ERROR = "GraphicsManager must be initialized.";
	private Exception NotInitializedException = new InvalidOperationException(INIT_ERROR);

	public OrthographicCamera GetCamera() => _camera ?? throw NotInitializedException;
	public Effect GetEffect(string name) => _effects[name.ToLower()];
	public GraphicsDevice GetGraphicsDevice() => _graphicsDeviceManager.GraphicsDevice ?? throw NotInitializedException;
	public SpriteBatch GetSpriteBatch() => _spriteBatch ?? throw NotInitializedException;
	public RenderTarget2D GetRenderTarget(string name) => _renderTargets[name.ToLower()];

	public Effect RegisterEffect(Effect effect)
	{
		string name = effect.Name.Split("/",2)[1].ToLower();
		_effects.Add(name, effect);
		return _effects[name];
	}

	public override string ToString() => $"Viewport:{ViewportResolution} Virtual:{VirtualResolution} RenderTarget:{RenderTarget.Bounds} DestinationRectangle:{DestinationRectangle}";

	public static readonly Size2 BaseResolution;
	public static readonly Dictionary<ushort, (string ArgumentsLiteral, Size2 ScaleFactor, Size2 Resolution)> DisplayModes;

	static GraphicsManager()
	{
		BaseResolution = new(320, 180);
		DisplayModes = new();

		// Base resolution (16:9): 320 x 180
        // Scale factors: 1x, 2x, 4x, 6x, 12x, 24x
		DisplayModes.Add(((ushort)GraphicsMode.Default), (
			"Default",
			new(
				GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / BaseResolution.Width, 
				GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / BaseResolution.Height
			),
			new(
			GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
			GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
		)));
		DisplayModes.Add(((ushort)GraphicsMode.NES), (
			"'--nes'",
			new(266 / BaseResolution.Width, 240 / BaseResolution.Height),
			new(266, 240)
		));
		DisplayModes.Add(((ushort)GraphicsMode.SNES), (
			"'--snes'",
			new(256 / BaseResolution.Width, 224 / BaseResolution.Height),
			new(256, 224)
		));
		DisplayModes.Add(((ushort)GraphicsMode.Genesis),	(
			"'--genesis'",
			new(320 / BaseResolution.Width, 244 / BaseResolution.Height),
			new(320, 244)
		));
		DisplayModes.Add(((ushort)GraphicsMode.OneEightyP), ("'--180p'", new(1, 1), BaseResolution * 1));
		DisplayModes.Add(((ushort)GraphicsMode.ThreeSixtyP), ("'--360p'", new(2, 2), BaseResolution * 2));
		DisplayModes.Add(((ushort)GraphicsMode.FourFiftyP), (
			"'--450p'",
			new(800 / BaseResolution.Width, 450 / BaseResolution.Height),
			new(800, 450)
		));
		DisplayModes.Add(((ushort)GraphicsMode.SevenTwentyP), ("'--720p'", new(4, 4), BaseResolution * 4));
		DisplayModes.Add(((ushort)GraphicsMode.ThousandEightyP), ("'--1080p'", new(6, 6), BaseResolution * 6));
		DisplayModes.Add(((ushort)GraphicsMode.FourK), ("'--4K'", new(12, 12), BaseResolution * 12));
		DisplayModes.Add(((ushort)GraphicsMode.EightK), ("'--8K'", new(24, 24), BaseResolution * 24));
	}

	public override void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if(_isDisposed)
			return;

		if(disposing)
		{
			foreach (Effect effect in _effects.Values)
				effect?.Dispose();

			_graphicsDeviceManager.Dispose();
			_spriteBatch?.Dispose();
			_renderTarget.Dispose();

			foreach (RenderTarget2D renderTarget in _renderTargets.Values)
				renderTarget?.Dispose();

			_viewportAdapter.Dispose();

			Console.WriteLine("GraphicsManager.Dispose() => OK");
		}

		_isDisposed = true;
	}

	~GraphicsManager() => this.Dispose(false);
}