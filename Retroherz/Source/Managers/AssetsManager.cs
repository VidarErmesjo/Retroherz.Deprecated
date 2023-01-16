using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Documents;
using MonoGame.Extended.Tiled;

namespace Retroherz.Managers;

public class AssetsManager : IDisposable
{
	private bool _isDisposed;

	private readonly ContentManager _content;
	private Dictionary<string, Effect> _effects = new();
	private Dictionary<string, SpriteFont> _fonts = new();
	private Dictionary<string, TiledMap> _maps = new();
	private Dictionary<string, AsepriteDocument> _sprites = new();
	private Dictionary<string, Texture2D> _textures = new();

	public AssetsManager(ContentManager content) => _content = content;

	private string AssetsPath(string path, string filename, string rootDir) =>
		Path.Combine(path.Substring(path.IndexOf(rootDir) + rootDir.Length), filename).Replace('\\', '/').Substring(1);

	public Effect Effect(string name) => _effects[name];
	public SpriteFont Font(string name) => _fonts[name];
	public TiledMap Map(string name) => _maps[name];
	public AsepriteDocument Sprite(string name) => _sprites[name];
	public Texture2D Texture(string name) => _textures[name];

	public void LoadContent(string rootDirectory = "Content")
	{
		_content.RootDirectory = rootDirectory;
		if (_effects.Count == 0)
			_effects = Load<Effect>("Effects");

		/*if (_maps.Count == 0)
			_maps = Load<TiledMap>("Tiled");*/

/*
Unhandled exception. Microsoft.Xna.Framework.Content.ContentLoadException: The content file was not found.
 ---> System.IO.FileNotFoundException: Could not find file 'C:\Users\Voidar\Documents\Programmering\dotnet\MonoGame\Retroherz\Retroherz\bin\Debug\net7.0-windows\Content\Tiled\Shitbrick.xnb'.
File name: 'C:\Users\Voidar\Documents\Programmering\dotnet\MonoGame\Retroherz\Retroherz\bin\Debug\net7.0-windows\Content\Tiled\Shitbrick.xnb'
*/

		if (_sprites.Count == 0)
			_sprites = Load<AsepriteDocument>("Aseprite");

		if (_textures.Count == 0)
			_textures = Load<Texture2D>("Textures");

		if (_fonts.Count == 0)
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
		if(_isDisposed)
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

		_isDisposed = true;
	}

	~AssetsManager()
	{
		Dispose(false);
	}
}