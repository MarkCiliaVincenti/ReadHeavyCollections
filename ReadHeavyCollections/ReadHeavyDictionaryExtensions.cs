using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace ReadHeavyCollections;

/// <summary>
/// Provides a set of initialization methods for instances of the <see cref="ReadHeavyDictionary{TKey, TValue}"/> class.
/// </summary>
public static class ReadHeavyDictionary
{
    extension<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
    {
        /// <summary>Creates a <see cref="ReadHeavyDictionary{TKey, TValue}"/> with the specified key/value pairs.</summary>
        /// <param name="comparer">The comparer implementation to use to compare keys for equality. If null, <see cref="EqualityComparer{TKey}.Default"/> is used.</param>
        /// <remarks>
        /// If the same key appears multiple times in the input, the latter one in the sequence takes precedence. This differs from
        /// <see cref="M:System.Linq.Enumerable.ToDictionary"/>, with which multiple duplicate keys will result in an exception.
        /// </remarks>
        /// <returns>A <see cref="ReadHeavyDictionary{TKey, TValue}"/> that contains the specified keys and values.</returns>
        public ReadHeavyDictionary<TKey, TValue> ToReadHeavyDictionary(IEqualityComparer<TKey>? comparer = null)
            => comparer is null ? new(source) : new(source, comparer);
    }

    extension<TSource>(IEnumerable<TSource> source)
    {
        /// <summary>Creates a <see cref="FrozenDictionary{TKey, TSource}"/> from an <see cref="IEnumerable{TSource}"/> according to specified key selector function.</summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>A <see cref="FrozenDictionary{TKey, TElement}"/> that contains the keys and values selected from the input sequence.</returns>
        public ReadHeavyDictionary<TKey, TSource> ToReadHeavyDictionary<TKey>(
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
                => comparer is null ? source.ToDictionary(keySelector).ToReadHeavyDictionary() : source.ToDictionary(keySelector, comparer).ToReadHeavyDictionary(comparer);

        /// <summary>Creates a <see cref="FrozenDictionary{TKey, TElement}"/> from an <see cref="IEnumerable{TSource}"/> according to specified key selector and element selector functions.</summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>A <see cref="FrozenDictionary{TKey, TElement}"/> that contains the keys and values selected from the input sequence.</returns>
        public ReadHeavyDictionary<TKey, TElement> ToReadHeavyDictionary<TKey, TElement>(
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
                => comparer is null ? source.ToDictionary(keySelector, elementSelector).ToReadHeavyDictionary() : source.ToDictionary(keySelector, elementSelector, comparer).ToReadHeavyDictionary(comparer);
    }
}