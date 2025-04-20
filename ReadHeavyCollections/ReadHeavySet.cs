using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.ComponentModel;
#endif
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

namespace ReadHeavyCollections;

/// <summary>
/// Represents a set of values that is highly optimized for read but performs poorly in write operations.
/// </summary>
/// <typeparam name="T">The type for the values.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[Serializable]
public sealed class ReadHeavySet<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISet<T>, ICollection, IReadOnlySet<T>, ISerializable, IDeserializationCallback
{
    private readonly Lock _lock = new();
    private FrozenSet<T> _frozenSet;
    private readonly HashSet<T> _hashSet;
    private readonly bool _isComparerSet;
    private readonly IEqualityComparer<T>? _comparer;

    #region Constructors
    /// <summary>
    /// Creates an empty <see cref="ReadHeavySet{T}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavySet()
    {
        _isComparerSet = false;
        _hashSet = [];
        _frozenSet = _hashSet.ToFrozenSet();
    }

    /// <summary>
    /// Creates a <see cref="ReadHeavySet{T}"/> with the specified input sequence.
    /// </summary>
    /// <param name="collection">An input sequence.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavySet(IEnumerable<T> collection)
    {
        _isComparerSet = false;
        _hashSet = collection.ToHashSet();
        _frozenSet = _hashSet.ToFrozenSet();
    }

    /// <summary>
    /// Creates an empty <see cref="ReadHeavySet{T}"/> with the provided comparer.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavySet(IEqualityComparer<T> comparer)
    {
        _isComparerSet = true;
        _comparer = comparer;
        _hashSet = new(comparer);
        _frozenSet = _hashSet.ToFrozenSet(comparer);
    }

    /// <summary>
    /// Creates a <see cref="ReadHeavySet{T}"/> from the provided set.
    /// </summary>
    /// <param name="set">The set.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavySet(ISet<T> set)
    {
        _isComparerSet = false;
        _hashSet = [.. set];
        _frozenSet = _hashSet.ToFrozenSet();
    }

    /// <summary>
    /// Creates a <see cref="ReadHeavySet{T}"/> with the specified input sequence and provided comparer.
    /// </summary>
    /// <param name="collection">An input sequence.</param>
    /// <param name="comparer">The comparer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavySet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        _isComparerSet = true;
        _comparer = comparer;
        _hashSet = collection.ToHashSet(comparer);
        _frozenSet = _hashSet.ToFrozenSet(comparer);
    }

    /// <summary>
    /// Creates a <see cref="ReadHeavySet{T}"/> from the provided set and comparer.
    /// </summary>
    /// <param name="set">The input sequence.</param>
    /// <param name="comparer">The comparer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavySet(ISet<T> set, IEqualityComparer<T> comparer)
    {
        _isComparerSet = true;
        _comparer = comparer;
        _hashSet = set.ToHashSet(comparer);
        _frozenSet = _hashSet.ToFrozenSet(comparer);
    }
    #endregion

    #region PublicExtraMethods
    /// <summary>
    /// Adds multiple entries to the <see cref="ReadHeavySet{T}"/> in a more optimal way than multiple <see cref="ReadHeavySet{T}.Add(T)"/> calls.
    /// </summary>
    /// <param name="items">An input sequence to add to the set.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(IEnumerable<T> items)
    {
        lock (_lock)
        {
            foreach (var item in items)
            {
                _hashSet.Add(item);
            }
            Freeze();
        }
    }
    #endregion

    #region HelperMethods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Freeze()
    {
        _frozenSet = (_isComparerSet) ? _hashSet.ToFrozenSet(_comparer) : _hashSet.ToFrozenSet();
    }
    #endregion

    #region ICollection<T>
    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.Count"/>
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenSet.Count;
    }

    bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.Add(T)"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns><inheritdoc cref="HashSet{T}.Add(T)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item)
    {
        lock (_lock)
        {
            if (_hashSet.Add(item))
            {
                Freeze();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.Add(T)"/>
    /// </summary>
    /// <param name="item"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.Clear"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        lock (_lock)
        {
            _hashSet.Clear();
            Freeze();
        }
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.Contains(T)"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns><inheritdoc cref="FrozenSet{T}.Contains(T)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item) => _frozenSet.Contains(item);

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.CopyTo(T[], int)"/>
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex) => _frozenSet.CopyTo(array, arrayIndex);

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.Remove(T)"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns><inheritdoc cref="HashSet{T}.Remove(T)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        bool removed;
        lock (_lock)
        {
            removed = _hashSet.Remove(item);
            if (removed)
            {
                Freeze();
            }
        }
        return removed;
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.GetEnumerator"/>
    /// </summary>
    /// <returns><inheritdoc cref="FrozenSet{T}.GetEnumerator"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator() => _frozenSet.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => _frozenSet.GetEnumerator();
    #endregion

    #region ISet<T>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool ISet<T>.Add(T item)
    {
        bool added;
        lock (_lock)
        {
            added = _hashSet.Add(item);
            if (added)
            {
                Freeze();
            }
        }
        return added;
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.ExceptWith(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExceptWith(IEnumerable<T> other)
    {
        lock (_lock)
        {
            _hashSet.ExceptWith(other);
            Freeze();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IntersectWith(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IntersectWith(IEnumerable<T> other)
    {
        lock (_lock)
        {
            _hashSet.IntersectWith(other);
            Freeze();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IsProperSubsetOf(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="HashSet{T}.IsProperSubsetOf(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return _frozenSet.IsProperSubsetOf(other);
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IsProperSupersetOf(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="HashSet{T}.IsProperSupersetOf(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return _frozenSet.IsProperSupersetOf(other);
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IsSubsetOf(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="HashSet{T}.IsSubsetOf(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        return _frozenSet.IsSubsetOf(other);
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IsSupersetOf(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="HashSet{T}.IsSupersetOf(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return _frozenSet.IsSupersetOf(other);
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.Overlaps(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="FrozenSet{T}.Overlaps(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Overlaps(IEnumerable<T> other)
    {
        return _frozenSet.Overlaps(other);
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.SetEquals(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="FrozenSet{T}.SetEquals(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SetEquals(IEnumerable<T> other)
    {
        return _frozenSet.SetEquals(other);
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.SymmetricExceptWith(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        lock (_lock)
        {
            _hashSet.SymmetricExceptWith(other);
            Freeze();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.UnionWith(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnionWith(IEnumerable<T> other)
    {
        lock (_lock)
        {
            _hashSet.UnionWith(other);
            Freeze();
        }
    }
    #endregion

    #region ICollection
    bool ICollection.IsSynchronized => true;

    object ICollection.SyncRoot => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection.CopyTo(Array array, int index)
    {
        _hashSet.ToArray().CopyTo(array, index);
    }
    #endregion

    #region ISerializable
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="info"><inheritdoc /></param>
    /// <param name="context"><inheritdoc /></param>
#if NET8_0_OR_GREATER
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetObjectData(SerializationInfo info, StreamingContext context) => _hashSet.GetObjectData(info, context);
    #endregion

    #region IDeserializationCallback
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="sender"><inheritdoc /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnDeserialization(object? sender)
    {
        lock (_lock)
        {
            _hashSet.OnDeserialization(sender);
            Freeze();
        }
    }
    #endregion
}