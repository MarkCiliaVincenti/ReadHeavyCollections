using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
#if NET9_0_OR_GREATER
using System.ComponentModel;
#endif
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

namespace ReadHeavyCollections;

/// <summary>
/// Represents a collection of keys and values that is highly optimized for read but performs poorly in write operations.
/// </summary>
/// <typeparam name="TKey">The non-nullable type for the keys.</typeparam>
/// <typeparam name="TValue">The type for the values.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[Serializable]
public sealed class ReadHeavyDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, ICollection, IDictionary, ISerializable, IDeserializationCallback where TKey : notnull
{
    private readonly Lock _lock = new();
    private FrozenDictionary<TKey, TValue> _frozenDictionary;
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly bool _isComparerSet;
    private readonly IEqualityComparer<TKey> _comparer;

    #region Constructors
    /// <summary>
    /// Creates an empty <see cref="ReadHeavyDictionary{TKey, TValue}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavyDictionary()
    {
        _isComparerSet = false;
        _dictionary = [];
    }

    /// <summary>
    /// Creates a <see cref="ReadHeavyDictionary{TKey, TValue}"/> with the specified key/value pairs.
    /// </summary>
    /// <param name="collection">A collection of key/value pairs.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    {
        _isComparerSet = false;
        _dictionary = collection.ToDictionary(x => x.Key, x => x.Value);
        Freeze();
    }

    /// <summary>
    /// Creates an empty <see cref="ReadHeavyDictionary{TKey, TValue}"/> with the provided comparer.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavyDictionary(IEqualityComparer<TKey> comparer)
    {
        _isComparerSet = true;
        _comparer = comparer;
        _dictionary = new(comparer);
    }

    /// <summary>
    /// Creates a <see cref="ReadHeavyDictionary{TKey, TValue}"/> from the provided dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavyDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _isComparerSet = false;
        _dictionary = dictionary.ToDictionary(x => x.Key, x => x.Value);
        Freeze();
    }

    /// <summary>
    /// Creates a <see cref="ReadHeavyDictionary{TKey, TValue}"/> with the specified key/value pairs and provided comparer.
    /// </summary>
    /// <param name="collection">A collection of key/value pairs.</param>
    /// <param name="comparer">The comparer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
    {
        _isComparerSet = true;
        _comparer = comparer;
        _dictionary = collection.ToDictionary(x => x.Key, x => x.Value, comparer);
        Freeze();
    }

    /// <summary>
    /// Creates a <see cref="ReadHeavyDictionary{TKey, TValue}"/> from the provided dictionary and comparer.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="comparer">The comparer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadHeavyDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
    {
        _isComparerSet = true;
        _comparer = comparer;
        _dictionary = dictionary.ToDictionary(x => x.Key, x => x.Value, comparer);
        Freeze();
    }
    #endregion

    #region PublicExtraMethods
    /// <summary>
    /// Adds multiple entries to the <see cref="ReadHeavyDictionary{TKey, TValue}"/> in a more optimal way than multiple <see cref="ReadHeavyDictionary{TKey, TValue}.Add(TKey, TValue)"/> calls.
    /// </summary>
    /// <param name="items">A key/value collection to add to the dictionary.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        lock (_lock)
        {
            foreach (var item in items)
            {
                _dictionary.Add(item.Key, item.Value);
            }
            Freeze();
        }
    }

    /// <summary>
    /// Removes the value with the specified key from the <see cref="ReadHeavyDictionary{TKey, TValue}"/>, and copies the element to the value parameter.
    /// </summary>
    /// <param name="key">The key of the item to remove</param>
    /// <param name="value">The value of the removed parameter.</param>
    /// <returns>true if the item is succesfully found and removed; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        bool removed;
        lock (_lock)
        {
            removed = _dictionary.Remove(key, out value);
            if (removed)
            {
                Freeze();
            }
        }
        return removed;
    }
    #endregion

    #region HelperMethods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Freeze()
    {
        _frozenDictionary = (_isComparerSet) ? _dictionary.ToFrozenDictionary(_comparer) : _dictionary.ToFrozenDictionary();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCompatibleKey(object key) => key == null ? throw new ArgumentNullException(nameof(key)) : key is TKey;
    #endregion

    #region ICollection<KeyValuePair<TKey, TValue>>
    /// <summary>
    /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.Count"/>
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenDictionary.Count;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        lock (_lock)
        {
            _dictionary.Add(item.Key, item.Value);
            Freeze();
        }
    }

    /// <summary>
    /// Removes all keys and values from the <see cref="ReadHeavyDictionary{TKey, TValue}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        lock (_lock)
        {
            _dictionary.Clear();
            Freeze();
        }
    }

    /// <summary>
    /// Determines whether a sequence contains a specified element by using the default equality comparer.
    /// </summary>
    /// <param name="item">The value to locate in the sequence.</param>
    /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(KeyValuePair<TKey, TValue> item) => _frozenDictionary.Contains(item);

    /// <summary>
    /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.CopyTo(KeyValuePair{TKey, TValue}[], int)"/>
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _frozenDictionary.CopyTo(array, arrayIndex);

    /// <summary>
    /// <inheritdoc cref="Dictionary{TKey, TValue}.Remove(TKey)"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key)
    {
        bool removed;
        lock (_lock)
        {
            removed = _dictionary.Remove(key);
            if (removed)
            {
                Freeze();
            }
        }
        return removed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair) => Remove(keyValuePair.Key);

    /// <summary>
    /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.GetEnumerator()"/>
    /// </summary>
    /// <returns><inheritdoc cref="FrozenDictionary{TKey, TValue}.GetEnumerator()"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _frozenDictionary.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => _frozenDictionary.GetEnumerator();
    #endregion

    #region IDictionary<TKey, TValue>
    /// <summary>
    /// <inheritdoc cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/>
    /// </summary>
    /// <param name="key">The key for the item to add.</param>
    /// <param name="value">The value for the item to add.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="ArgumentException" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value)
    {
        lock (_lock)
        {
            _dictionary.Add(key, value);
            Freeze();
        }
    }

    /// <summary>
    /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.ContainsKey(TKey)"/>
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns><inheritdoc cref="FrozenDictionary{TKey, TValue}.ContainsKey(TKey)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) => _frozenDictionary.ContainsKey(key);

    /// <summary>
    /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
    /// </summary>
    /// <param name="key">The key to find.</param>
    /// <param name="value">The value.</param>
    /// <returns><inheritdoc cref="FrozenDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _frozenDictionary.TryGetValue(key, out value);
    #endregion

    #region IReadOnlyDictionary<TKey, TValue>
    /// <summary>
    /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.Keys"/>
    /// </summary>
    public ICollection<TKey> Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenDictionary.Keys;
    }

    /// <summary>
    /// <inheritdoc cref="FrozenDictionary{TKey, TValue}.Values"/>
    /// </summary>
    public ICollection<TValue> Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenDictionary.Values;
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenDictionary.Keys;
    }

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenDictionary.Values;
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="key"><inheritdoc /></param>
    /// <returns><inheritdoc /></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TValue this[TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenDictionary[key];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (_lock)
            {
                _dictionary[key] = value;
                Freeze();
            }
        }
    }
    #endregion

    #region ICollection
    bool ICollection.IsSynchronized => true;

    object ICollection.SyncRoot => this;

    void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();
    #endregion

    #region IDictionary
    bool IDictionary.IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;

    ICollection IDictionary.Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenDictionary.Keys;
    }

    ICollection IDictionary.Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _frozenDictionary.Values;
    }

    object IDictionary.this[object key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (IsCompatibleKey(key))
            {
                return _frozenDictionary[(TKey)key];
            }

            return null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ArgumentNullException.ThrowIfNull(key);

            try
            {
                TKey tempKey = (TKey)key;
                try
                {
                    this[tempKey] = (TValue)value!;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException(value!.ToString(), typeof(TValue).FullName);
                }
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(key.ToString(), typeof(TKey).FullName);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IDictionary.Add(object key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            TKey tempKey = (TKey)key;
            try
            {
                this[tempKey] = (TValue)value!;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(value!.ToString(), typeof(TValue).FullName);
            }
        }
        catch (InvalidCastException)
        {
            throw new ArgumentException(key.ToString(), typeof(TKey).FullName);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IDictionary.Contains(object key)
    {
        if (IsCompatibleKey(key))
        {
            return ContainsKey((TKey)key);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IDictionaryEnumerator IDictionary.GetEnumerator() => _dictionary.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IDictionary.Remove(object key)
    {
        if (IsCompatibleKey(key))
        {
            Remove((TKey)key);
        }
    }
    #endregion

    #region ISerializable
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="info"><inheritdoc /></param>
    /// <param name="context"><inheritdoc /></param>
#if NET9_0_OR_GREATER
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetObjectData(SerializationInfo info, StreamingContext context) => _dictionary.GetObjectData(info, context);
    #endregion

    #region IDeserializationCallback
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="sender"><inheritdoc /></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnDeserialization(object sender)
    {
        lock (_lock)
        {
            _dictionary.OnDeserialization(sender);
            Freeze();
        }
    }
    #endregion
}
