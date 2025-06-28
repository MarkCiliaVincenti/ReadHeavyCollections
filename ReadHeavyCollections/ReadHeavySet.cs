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
    private readonly ReaderWriterLockSlim _lock = new();
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
        _hashSet = [.. collection];
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
        if (items.Any())
        {
            _lock.EnterWriteLock();
            try
            {
                bool added = false;
                foreach (var item in items)
                {
                    if (_hashSet.Add(item))
                    {
                        added = true;
                    }
                }
                if (added)
                {
                    Freeze();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
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
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _frozenSet.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
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
        _lock.EnterWriteLock();
        try
        {
            if (_hashSet.Add(item))
            {
                Freeze();
                return true;
            }
            return false;
        }
        finally
        {
            _lock.ExitWriteLock();
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
        _lock.EnterWriteLock();
        try
        {
            _hashSet.Clear();
            Freeze();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.Contains(T)"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns><inheritdoc cref="FrozenSet{T}.Contains(T)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.Contains(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.CopyTo(T[], int)"/>
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex)
    {
        _lock.EnterReadLock();
        try
        {
            _frozenSet.CopyTo(array, arrayIndex);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.Remove(T)"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns><inheritdoc cref="HashSet{T}.Remove(T)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        bool removed;
        _lock.EnterWriteLock();
        try
        {
            removed = _hashSet.Remove(item);
            if (removed)
            {
                Freeze();
            }
            return removed;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.GetEnumerator"/>
    /// </summary>
    /// <returns><inheritdoc cref="FrozenSet{T}.GetEnumerator"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.GetEnumerator();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.GetEnumerator();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    #endregion

    #region ISet<T>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool ISet<T>.Add(T item)
    {
        bool added;
        _lock.EnterWriteLock();
        try
        {
            added = _hashSet.Add(item);
            if (added)
            {
                Freeze();
            }
            return added;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.ExceptWith(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExceptWith(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.ExceptWith(other);
            Freeze();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IntersectWith(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IntersectWith(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.IntersectWith(other);
            Freeze();
        }
        finally
        {
            _lock.ExitWriteLock();
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
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.IsProperSubsetOf(other);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IsProperSupersetOf(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="HashSet{T}.IsProperSupersetOf(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.IsProperSupersetOf(other);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IsSubsetOf(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="HashSet{T}.IsSubsetOf(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.IsSubsetOf(other);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.IsSupersetOf(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="HashSet{T}.IsSupersetOf(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.IsSupersetOf(other);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.Overlaps(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="FrozenSet{T}.Overlaps(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Overlaps(IEnumerable<T> other)
    {
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.Overlaps(other);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="FrozenSet{T}.SetEquals(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns><inheritdoc cref="FrozenSet{T}.SetEquals(IEnumerable{T})"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SetEquals(IEnumerable<T> other)
    {
        _lock.EnterReadLock();
        try
        {
            return _frozenSet.SetEquals(other);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.SymmetricExceptWith(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.SymmetricExceptWith(other);
            Freeze();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// <inheritdoc cref="HashSet{T}.UnionWith(IEnumerable{T})"/>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnionWith(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.UnionWith(other);
            Freeze();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    #endregion

    #region ICollection
    bool ICollection.IsSynchronized => true;

    object ICollection.SyncRoot => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection.CopyTo(Array array, int index)
    {
        _lock.EnterReadLock();
        try
        {
            ((ICollection)_frozenSet).CopyTo(array, index);
        }
        finally
        {
            _lock.ExitReadLock();
        }
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
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        _lock.EnterReadLock();
        try
        {
            _hashSet.GetObjectData(info, context);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    #endregion

    #region IDeserializationCallback
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="sender"><inheritdoc /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnDeserialization(object? sender)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.OnDeserialization(sender);
            Freeze();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    #endregion
}