using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using Retroherz.Math;
using Retroherz.Systems;

namespace Retroherz.Components;

/// <summary>
/// Point light that casts shadows
/// </summary>
public class VisibilityComponent
{
	public VertexPositionTexture[] Vertices;
	public short[] Indices;

	/// <summary>
	/// Radius of influence of the light.
	/// </summary>
	public float Radius { get; set; }

	/// <summary>
	/// Color of the light
	/// </summary>
	public Color Color { get; set; }

	/// <summary>
	/// Power of the light, from 0 (turned off) to 
	/// 1 for maximum brightness. 
	/// </summary>
	public float Power { get; set; }

	public VisibilityComponent(float radius, Color color, float power = 1)
	{
		Radius = radius;
		Color = color;
		Power = power;            
	}
}