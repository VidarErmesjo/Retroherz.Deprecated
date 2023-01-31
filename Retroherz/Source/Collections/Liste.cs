using System;

namespace Retroherz.Collections;

///	<summary>
///	An array based on <see cref="Span" />.
/// EXPERIMENTAL. DO NOT USE!
///	</summary>
public ref struct Liste<T> where T: struct
{
	private readonly Span<T> _data;
	private readonly int _width;
	private readonly int _height;

	public void Sort(Comparison<T> comparison) => _data.Sort(comparison);
	public void Fill(T value) => _data.Fill(value);
	public int Length => _data.Length;

	public Liste(int length) => new Liste<T>(length, 1);

	public Liste(int width, int height)
	{
		_data = new T[width * height].AsSpan();
		_width = width;
		_height = height;
	}

	public T this[int x, int y]
	{
		get => _data[y * _width + x];
		set => _data[y * _width + x] = value;
	}

	public T this[int index]
	{
		get => _data[index];
		set => _data[index] = value;
	}
}