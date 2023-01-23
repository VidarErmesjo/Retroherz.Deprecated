using System;
using System.Linq;

namespace MonoGame.Extended.Collections;

public static class CollectionsExtensions
{
	public static ReadOnlySpan<T> AsReadOnlySpan<T>(this Bag<T> bag) => bag.ToArray().AsSpan();
	public static Span<T> AsSpan<T>(this Bag<T> bag) => bag.ToArray().AsSpan();	
}

