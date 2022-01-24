using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite.Documents;
using MonoGame.Aseprite.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoGame.Assets
{
    interface IAssetsManager : IDisposable
    {
    }

    public class AssetsManager : IAssetsManager
    {
        private bool isDisposed;

        private readonly ContentManager _content;
        private Dictionary<string, Effect> _effects;
        private Dictionary<string, Texture2D> _textures;
        private Dictionary<string, SpriteFont> _fonts;
        private Dictionary<string, AsepriteDocument> _sprites;

        public AssetsManager(ContentManager content)
        {
            _content = content;
            _effects = new Dictionary<string, Effect>();
            _textures = new Dictionary<string, Texture2D>();
            _fonts = new Dictionary<string, SpriteFont>();
            _sprites = new Dictionary<string, AsepriteDocument>();
        }

        private string AssetsPath(string path, string filename, string rootDir) =>
            Path.Combine(path.Substring(path.IndexOf(rootDir) + rootDir.Length), filename).Replace('\\', '/').Substring(1);

        public Effect Effect(string name) => _effects[name];

        public Texture2D Texture(string name) => _textures[name];

        public SpriteFont Font(string name) => _fonts[name];
     
        public AsepriteDocument Sprite(string name) => _sprites[name];

        public void LoadContent()
        {
            if(_effects.Count() == 0)
                _effects = Load<Effect>("Effects");

            if(_textures.Count() == 0)
                _textures = Load<Texture2D>("Textures");

            if(_sprites.Count() == 0)
                _sprites = Load<AsepriteDocument>("Aseprite");

            if(_fonts.Count() == 0)
                _fonts = Load<SpriteFont>("Fonts");
        }

        public Dictionary<string, T> Load<T>(string directory)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(_content.RootDirectory, directory));
            if (!dir.Exists)
                throw new DirectoryNotFoundException();

            Dictionary<string, T> tmp = new Dictionary<string, T>();
            FileInfo[] files = dir.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                string asset = AssetsPath(
                    file.DirectoryName,
                    Path.GetFileNameWithoutExtension(file.Name),
                    _content.RootDirectory);

                System.Console.WriteLine(
                    "{0}.Load<T>() => Loaded",
                    asset.Split('/').Last());
                    //asset.GetType().FullName);    
                            
                tmp.Add(asset.Split('/').Last(), _content.Load<T>(asset));
            }

            return tmp;
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
                _content.Dispose();
                
                foreach(var effect in _effects)
                {
                    effect.Value.Dispose();
                    System.Console.WriteLine("{0}.Dispose() => OK", effect.Key);
                }

                foreach(var texture in _textures)
                {
                    texture.Value.Dispose();
                    System.Console.WriteLine("{0}.Dispose() => OK", texture.Key);
                }

                foreach(var sprite in _sprites)
                {
                    sprite.Value.Dispose();
                    System.Console.WriteLine("{0}.Dispose() => OK", sprite.Key);
                }
            }

            isDisposed = true;
        }

        ~AssetsManager()
        {
            Dispose(false);
        }
    }
}