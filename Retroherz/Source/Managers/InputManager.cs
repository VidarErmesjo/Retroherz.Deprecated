using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Retroherz.Math;

namespace Retroherz
{
	public enum InputManagerState
	{
		Idle,
		MouseDragStart,
		MouseDragEnd,
		MouseDragHold
	}

	public class InputManager : SimpleGameComponent
	{
		private readonly OrthographicCamera _camera;
		private MouseState _currentMouseState;
		private MouseState _previousMouseState;
		private Nullable<Vector2> _mouseDragStart;
		private Nullable<Vector2> _mouseDragEnd;
		private Nullable<RectangleF> _selection;

		public MouseState CurrentMouseState => _currentMouseState;
		public MouseState PreviousMouseState => _previousMouseState;
		public RectangleF Selection => _selection ?? RectangleF.Empty;

		public InputManagerState State { get; private set; }

		public InputManager(OrthographicCamera camera)
		{
			_camera = camera;
			_currentMouseState = new();
			_previousMouseState = new();
			_mouseDragStart = null;
			_mouseDragEnd = null;
			_selection = null;

			State = InputManagerState.Idle;
		}

		private void OnMouseDragStart()
		{
			_mouseDragStart = _camera.ScreenToWorld(_currentMouseState.Position.ToVector2());
		}

		private void OnMouseDragEnd()
		{
			_mouseDragEnd = _camera.ScreenToWorld(_currentMouseState.Position.ToVector2());

			var start = _mouseDragStart ?? Vector2.Zero;
			var end = _mouseDragEnd ?? Vector2.Zero;

			_selection = Utils.ToRectangleF(in start, in end);

			_mouseDragStart = _mouseDragEnd = null;
		}

		private void OnMouseDragHold()
		{
			var start = _mouseDragStart ?? Vector2.Zero;
			var end = _camera.ScreenToWorld(_currentMouseState.Position.ToVector2());

			_selection = Utils.ToRectangleF(in start, in end);
		}

		private void OnMouseIdle()
		{
			var position = _camera.ScreenToWorld(_currentMouseState.Position.ToVector2());
			var size = new Size2(1, 1);

			_selection = new(position, size);
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			_previousMouseState = _currentMouseState;
			_currentMouseState = Mouse.GetState();

			// Determine state
			if (_currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
				State = InputManagerState.MouseDragStart;
			else if (_previousMouseState.RightButton == ButtonState.Pressed && _currentMouseState.RightButton == ButtonState.Released)
				State = InputManagerState.MouseDragEnd;
			else if (_mouseDragStart != null && _mouseDragEnd == null)
				State = InputManagerState.MouseDragHold;
			else
				State = InputManagerState.Idle;

			// Do
			switch (this.State)
			{
				case InputManagerState.Idle:
					this.OnMouseIdle();
					break;
				case InputManagerState.MouseDragStart:
					this.OnMouseDragStart();
					break;
				case InputManagerState.MouseDragHold:
					this.OnMouseDragHold();
					break;
				case InputManagerState.MouseDragEnd:
					this.OnMouseDragEnd();
					break;
				default:
					this.OnMouseIdle();
					break;
			}

			/*if (State != InputManagerState.Idle)
				System.Console.WriteLine(this.State);*/
		}

		~InputManager() {}
	
	}
}