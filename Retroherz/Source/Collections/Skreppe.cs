using System;

namespace Retroherz.Collections;

///	<summary>
///	An array based on <see cref="Span" />.
/// EXPERIMENTAL. DO NOT USE!
///	</summary>
ref struct Skreppe<T> where T: struct
{
	private readonly Span<T> _data;
	private int _count = 0;

	public int Count => _data.Slice(0, _count).Length;
	public void Sort(Comparison<T> comparison) => _data.Sort(comparison);
	public void Fill(T value) => _data.Fill(value);

	public Skreppe(int size = 16) => _data = new T[size].AsSpan();	// Array allocation here :/

	public T this[int index]
	{
		get => _data[index];
		set => _data[index] = value;
	}
}