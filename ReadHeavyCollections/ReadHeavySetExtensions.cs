using System;
using System.Collections.Generic;

namespace ReadHeavyCollections;

/// <summary>
/// Provides a set of initialization methods for instances of the <see cref="ReadHeavySet{T}"/> class.
/// </summary>
public static class ReadHeavySet
{
    /// <summary>Creates a <see cref="ReadHeavySet{T}"/> with the specified values.</summary>
    /// <param name="source">The values to use to populate the set.</param>
    /// <typeparam name="T">The type of the values in the set.</typeparam>
    /// <returns>A ReadHeavy set.</returns>
    public static ReadHeavySet<T> Create<T>(params ReadOnlySpan<T> source)
        => source.ToArray().ToReadHeavySet();

    /// <summary>Creates a <see cref="ReadHeavySet{T}"/> with the specified values.</summary>
    /// <param name="source">The values to use to populate the set.</param>
    /// <param name="equalityComparer">The comparer implementation to use to compare values for equality. If null, <see cref="EqualityComparer{T}.Default"/> is used.</param>
    /// <typeparam name="T">The type of the values in the set.</typeparam>
    /// <returns>A ReadHeavy set.</returns>
    public static ReadHeavySet<T> Create<T>(IEqualityComparer<T>? equalityComparer, params ReadOnlySpan<T> source)
        => source.ToArray().ToReadHeavySet(equalityComparer);

    extension<T>(IEnumerable<T> source)
    {
        /// <summary>Creates a <see cref="ReadHeavySet{T}"/> with the specified values.</summary>
        /// <param name="comparer">The comparer implementation to use to compare values for equality. If null, <see cref="EqualityComparer{T}.Default"/> is used.</param>
        /// <returns>A ReadHeavy set.</returns>
        public ReadHeavySet<T> ToReadHeavySet(IEqualityComparer<T>? comparer = null)
            => (comparer is null) ? new(source) : new(source, comparer);
    }
}