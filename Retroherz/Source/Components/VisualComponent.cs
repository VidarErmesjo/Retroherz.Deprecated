using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroherz.Components;
// Courtesy of Roy Triesscheijn - based on and modified by Vidar "Voidar" Ermesjø
// https://roy-t.nl/2014/02/27/2d-lighting-and-shadows-preview.html
// http://roy-t.nl/files/ShadowsIn2D.zip

public class VisualComponent
{
	/// <summary>
	/// The texture, or color of the object.
	/// </summary>
	public Texture2D Texture{ get; set; }

	/// <summary>
	/// The glow, or light emitted by the object.
	/// Can be null.
	/// </summary>
	public Texture2D Glow { get; set; }

	public VisualComponent(Texture2D texture, Texture2D glow)
	{
		Texture = texture;
		Glow = glow;
	}                
}