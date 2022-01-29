using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace Retroherz.Components
{
    public class TiledMapComponent
    {
        private readonly TiledMap _tiledMap;

        public TiledMap TiledMap { get => _tiledMap; }

        public TiledMapComponent(TiledMap tiledMap) { _tiledMap = tiledMap; }

        ~TiledMapComponent() {}
    }
}