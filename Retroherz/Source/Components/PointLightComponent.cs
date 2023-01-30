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
public class PointLightComponent
{
	/// <summary>
	///	Represents the renderable mesh of the influence of the light.
	/// </summary>
	public Primitive Primitive;

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

	public PointLightComponent(float radius = 1, Color color = default(Color), float power = 1)
	{
		Radius = radius;
		Color = color;
		Power = power;            
	}
}