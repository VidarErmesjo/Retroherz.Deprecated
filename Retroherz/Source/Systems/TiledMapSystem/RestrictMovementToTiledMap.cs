using Microsoft.Xna.Framework;
using MonoGame.Extended;

using Retroherz.Math;

namespace Retroherz.Systems
{
	public partial class TiledMapSystem
	{
		public void RestrictMovementToTiledMap(int entityId)
		{
			var collider = _colliderComponentMapper.Get(entityId);
			var transform = _transformComponentMapper.Get(entityId);

			if (collider.Type == Components.ColliderComponentType.Static) return;

			if (transform.Position.X < 0)
			{
				transform.Position = transform.Position.SetX(0);
				collider.Velocity = collider.Velocity.SetX(0);
			}
			else if (transform.Position.X > _tiledMap.WidthInPixels - collider.Size.X)
			{
				transform.Position = transform.Position.SetX(_tiledMap.WidthInPixels - collider.Size.X);
				collider.Velocity = collider.Velocity.SetX(0);
			}
			
			if (transform.Position.Y < 0)
			{
				transform.Position = transform.Position.SetY(0);
				collider.Velocity = collider.Velocity.SetY(0);
			}
			else if (transform.Position.Y > _tiledMap.HeightInPixels - collider.Size.Y)
			{
				transform.Position = transform.Position.SetY(_tiledMap.HeightInPixels - collider.Size.Y);
				collider.Velocity = collider.Velocity.SetY(0);
			}			
		}
	}
}