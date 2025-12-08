using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Xunit;

namespace ReadHeavyCollections.Tests;

public class ReadHeavyDictionaryTests
{
    [Fact]
    public void DictionaryConstructor_ShouldCopyItems()
    {
        var initial = new Dictionary<string, int>
        {
            { "a", 1 },
            { "b", 2 }
        };
        var dict = new ReadHeavyDictionary<string, int>(initial);
        dict.Count.Should().Be(2);
        dict["a"].Should().Be(1);
        dict["b"].Should().Be(2);
    }

    [Fact]
    public void DictionaryConstructorWithComparer_ShouldCopyItems()
    {
        var initial = new Dictionary<string, int>
        {
            { "a", 1 },
            { "b", 2 }
        };
        var dict = new ReadHeavyDictionary<string, int>(initial, StringComparer.OrdinalIgnoreCase);
        dict.Count.Should().Be(2);
        dict["a"].Should().Be(1);
        dict["b"].Should().Be(2);
    }

    [Fact]
    public void CollectionConstructorWithComparer_ShouldCopyItems()
    {
        var initial = new List<KeyValuePair<string, int>>
        {
            new("a", 1),
            new("b", 2)
        };
        var dict = new ReadHeavyDictionary<string, int>(initial, StringComparer.OrdinalIgnoreCase);
        dict.Count.Should().Be(2);
        dict["a"].Should().Be(1);
        dict["b"].Should().Be(2);
    }

    [Fact]
    public void CollectionConstructor_ShouldCopyItems()
    {
        var initial = new List<KeyValuePair<string, int>>
        {
            new("a", 1),
            new("b", 2)
        };
        var dict = new ReadHeavyDictionary<string, int>(initial);
        dict.Count.Should().Be(2);
        dict["a"].Should().Be(1);
        dict["b"].Should().Be(2);
    }

    [Fact]
    public void Add_ShouldInsertItem()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict.Add("one", 1);
        dict.Contains(new KeyValuePair<string, int>("one", 1)).Should().BeTrue();
        dict.Keys.Should().Contain("one");
        dict.Values.Should().Contain(1);
        dict["one"].Should().Be(1);
    }

    [Fact]
    public void Add_DuplicateKey_ShouldThrow()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict.Add("k", 1);
        FluentActions.Invoking(() => dict.Add("k", 2))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRange_ShouldNotThrow()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        var items = new List<KeyValuePair<string, int>>
        {
            new("one", 1),
            new("two", 2)
        };
        dict.AddRange(items);
        dict["one"].Should().Be(1);
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenKeyExists()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict.TryAdd("a", 1).Should().BeTrue();
        dict.TryAdd("a", 2).Should().BeFalse();
    }

    [Fact]
    public void Indexer_ShouldSetAndGet()
    {
        var dict = new ReadHeavyDictionary<string, string>
        {
            ["x"] = "abc"
        };
        dict["x"].Should().Be("abc");
    }

    [Fact]
    public void TryGetValue_ShouldWork()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            ["age"] = 42
        };

        dict.TryGetValue("age", out var value).Should().BeTrue();
        value.Should().Be(42);

        dict.TryGetValue("unknown", out _).Should().BeFalse();
    }

    [Fact]
    public void ContainsKey_ShouldReturnExpectedResult()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            ["present"] = 1
        };
        dict.ContainsKey("present").Should().BeTrue();
        dict.ContainsKey("missing").Should().BeFalse();
    }

    [Fact]
    public void Remove_ShouldWork()
    {
        var dict = new ReadHeavyDictionary<string, int> { ["x"] = 1 };
        dict.Remove("x").Should().BeTrue();
        dict.Remove("x").Should().BeFalse();
    }

    [Fact]
    public void RemoveWithOut_ShouldWork()
    {
        var dict = new ReadHeavyDictionary<string, int> { ["x"] = 1 };
        dict.Remove("x", out int value).Should().BeTrue();
        value.Should().Be(1);
        dict.Remove("x", out _).Should().BeFalse();
    }

#if NET9_0_OR_GREATER
    [Fact]
    public void Capacity_ShouldReturnExpectedValue()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict.Capacity.Should().Be(0);
    }
#endif

    [Fact]
    public void Clear_ShouldEmptyDictionary()
    {
        var dict = new ReadHeavyDictionary<int, string> { [1] = "a", [2] = "b" };
        dict.Clear();
        dict.Count.Should().Be(0);
    }

    [Fact]
    public void Dictionary_ShouldSupportCustomComparer()
    {
        var dict = new ReadHeavyDictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["HELLO"] = 10
        };
        dict.ContainsKey("hello").Should().BeTrue();
        dict.Comparer.Should().Be(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Enumerator_ShouldYieldSnapshot()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };

        var keys = dict.Select(kvp => kvp.Key).ToList();
        keys.Should().BeEquivalentTo(["a", "b"]);
    }

    [Fact]
    public void ConcurrentReads_ShouldBeThreadSafe()
    {
        var dict = new ReadHeavyDictionary<int, string>();
        for (int i = 0; i < 1000; i++)
            dict[i] = i.ToString();

        Parallel.For(0, 1000, i =>
        {
            dict.TryGetValue(i, out var val).Should().BeTrue();
            val.Should().Be(i.ToString());
        });
    }

    [Fact]
    public void ContainsValue_ShouldReturnExpectedResult()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            ["present"] = 1
        };
        dict.ContainsValue(1).Should().BeTrue();
        dict.ContainsValue(2).Should().BeFalse();
    }

    [Fact]
    public void EnsureCapacity_ShouldNotThrow()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict.EnsureCapacity(100);
#if NET9_0_OR_GREATER
        dict.Capacity.Should().BeGreaterThanOrEqualTo(100);
#endif
    }

    [Fact]
    public void TrimExcess_ShouldNotThrow()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict.TrimExcess();
    }

    [Fact]
    public void TrimExcessWithCapacity_ShouldNotThrow()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict.TrimExcess(100);
    }

    [Fact]
    public void GetValueRefOrNullRef_ShouldReturnExpectedValue()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            ["present"] = 42
        };

        ref readonly int presentRef = ref dict.GetValueRefOrNullRef("present");

        presentRef.Should().Be(42);

        Action action = () => {
            ref readonly int missingRef = ref dict.GetValueRefOrNullRef("missing");
            missingRef.Should().Be(null);
        };

        action.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void ThisNull_ShouldThrow()
    {
        Action action = () =>
        {
            var dict = new ReadHeavyDictionary<string, int>();
            dict[null!] = 2;
        };
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CopyTo_ShouldCopyItems()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };
        var array = new KeyValuePair<string, int>[2];
        dict.CopyTo(array, 0);
        array[0].Should().Be(new KeyValuePair<string, int>("a", 1));
        array[1].Should().Be(new KeyValuePair<string, int>("b", 2));
    }

    [Fact]
    public void ICollection_CopyTo_ShouldCopyItems()
    {
        ICollection dict = new ReadHeavyDictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };
        var array = new KeyValuePair<string, int>[2];
        dict.CopyTo(array, 0);
        array[0].Should().Be(new KeyValuePair<string, int>("a", 1));
        array[1].Should().Be(new KeyValuePair<string, int>("b", 2));
    }

    [Fact]
    public void OnDeserialization_ShouldNotThrow()
    {
        var dict = new ReadHeavyDictionary<string, int>();
#pragma warning disable SYSLIB0050 // Type or member is obsolete
        var info = new SerializationInfo(typeof(ReadHeavyDictionary<string, int>), new FormatterConverter());
#pragma warning restore SYSLIB0050 // Type or member is obsolete
        info.AddValue("Count", 0);
        info.AddValue("Comparer", StringComparer.OrdinalIgnoreCase);
        info.AddValue("Keys", new string[0]);
        info.AddValue("Values", new int[0]);
        Action action = () => dict.OnDeserialization(info);
        action.Should().NotThrow();
    }

    [Fact]
    public void ToReadHeavyDictionary_ShouldReturnExpectedValue()
    {
        var dict = new Dictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };
        var newDict = dict.ToReadHeavyDictionary();
        newDict.Should().BeEquivalentTo(dict);

        var dict2 = new ReadHeavyDictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = 1,
            ["b"] = 2
        };
        var newDict2 = dict2.ToReadHeavyDictionary(StringComparer.OrdinalIgnoreCase);
        newDict2.Should().BeEquivalentTo(dict2);

        {
            var source = new List<int>() { 1 };
            var keySelector = new Func<int, string>(i => i.ToString());

            var dictionary = source.ToDictionary(keySelector);
            var heavyDict = source.ToReadHeavyDictionary(keySelector);

            dictionary.Should().BeEquivalentTo(heavyDict);

            dictionary = source.ToDictionary(keySelector, StringComparer.OrdinalIgnoreCase);
            heavyDict = source.ToReadHeavyDictionary(keySelector, StringComparer.OrdinalIgnoreCase);

            dictionary.Should().BeEquivalentTo(heavyDict);
        }
        {
            var source = new List<int>() { 1 };
            var keySelector = new Func<int, string>(i => i.ToString());
            var elementSelector = new Func<int, int>(i => i + 1);

            var dictionary = source.ToDictionary(keySelector, elementSelector);
            var heavyDict = source.ToReadHeavyDictionary(keySelector, elementSelector);

            dictionary.Should().BeEquivalentTo(heavyDict);

            dictionary = source.ToDictionary(keySelector, elementSelector, StringComparer.OrdinalIgnoreCase);
            heavyDict = source.ToReadHeavyDictionary(keySelector, elementSelector, StringComparer.OrdinalIgnoreCase);

            dictionary.Should().BeEquivalentTo(heavyDict);
        }
    }

    [Fact]
    public void IDictionary_Interface_ShouldBehaveCorrectly()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        dict.Add("foo", 123);
        dict.Contains("foo").Should().BeTrue();
        dict["foo"].Should().Be(123);
        dict.Remove("foo");
        dict.Contains("foo").Should().BeFalse();
    }

    [Fact]
    public void IDictionary_Add_InvalidKeyOrValue_ShouldThrow()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        Action addNullKey = () => dict.Add(null!, 1);
        addNullKey.Should().Throw<ArgumentNullException>();

        Action addInvalidKey = () => dict.Add(123, 1);
        addInvalidKey.Should().Throw<ArgumentException>();

        Action addInvalidValue = () => dict.Add("foo", "bar");
        addInvalidValue.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Remove_KeyValuePair_ShouldWork()
    {
        var dict = new ReadHeavyDictionary<string, int> { ["a"] = 1 };
        ((ICollection<KeyValuePair<string, int>>)dict).Remove(new KeyValuePair<string, int>("a", 1)).Should().BeTrue();
        dict.Count.Should().Be(0);
    }

    [Fact]
    public void Enumerator_ShouldWorkWithForeach()
    {
        var dict = new ReadHeavyDictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var keys = new List<string>();
        foreach (var kvp in dict)
            keys.Add(kvp.Key);
        keys.Should().BeEquivalentTo(["a", "b"]);
    }

#if NET9_0_OR_GREATER
    [Fact]
    public void TryGetAlternateLookup_ShouldReturnFalse_WhenComparerDoesNotSupportAlternateKey()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        var result = dict.TryGetAlternateLookup<int>(out var lookup);
        result.Should().BeFalse();
    }
#endif

    [Fact]
    public void ICollectionOfKeyValuePair_IsReadOnly_ShouldBeFalse()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        var isReadOnly = ((ICollection<KeyValuePair<string, int>>)dict).IsReadOnly;
        isReadOnly.Should().BeFalse();
    }

    [Fact]
    public void ICollectionOfKeyValuePair_Add_ShouldAddItem()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        var collection = (ICollection<KeyValuePair<string, int>>)dict;
        collection.Add(new KeyValuePair<string, int>("foo", 42));
        dict["foo"].Should().Be(42);
    }

    [Fact]
    public void IEnumerable_GetEnumerator_ShouldReturnEnumerator()
    {
        var dict = new ReadHeavyDictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var enumerable = (IEnumerable)dict;
        var items = new List<KeyValuePair<string, int>>();
        foreach (KeyValuePair<string, int> kvp in enumerable)
            items.Add(kvp);
        items.Should().BeEquivalentTo(new[] { new KeyValuePair<string, int>("a", 1), new KeyValuePair<string, int>("b", 2) });
    }

    [Fact]
    public void IReadOnlyDictionary_Keys_And_Values_ShouldReturnExpected()
    {
        var dict = new ReadHeavyDictionary<string, int> { ["x"] = 10, ["y"] = 20 };
        var roDict = (IReadOnlyDictionary<string, int>)dict;
        roDict.Keys.Should().BeEquivalentTo(new[] { "x", "y" });
        roDict.Values.Should().BeEquivalentTo(new[] { 10, 20 });
    }

    [Fact]
    public void ICollection_IsSynchronized_ShouldBeTrue()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        var collection = (ICollection)dict;
        collection.IsSynchronized.Should().BeTrue();
    }

    [Fact]
    public void ICollection_SyncRoot_ShouldReturnSelf()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        var collection = (ICollection)dict;
        collection.SyncRoot.Should().BeSameAs(dict);
    }

    [Fact]
    public void IDictionary_Contains_ShouldReturnTrueForValidKey()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "foo", 1 } };
        dict.Contains("foo").Should().BeTrue();
    }

    [Fact]
    public void IDictionary_Contains_ShouldReturnFalseForMissingKey()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "foo", 1 } };
        dict.Contains("bar").Should().BeFalse();
    }

    [Fact]
    public void IDictionary_Contains_ShouldReturnFalseForWrongKeyType()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "foo", 1 } };
        dict.Contains(123).Should().BeFalse(); // int is not string
    }

    [Fact]
    public void IDictionary_Contains_ShouldThrowForNullKey()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "foo", 1 } };
        Action act = () => dict.Contains(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IDictionary_IndexerGet_ShouldReturnValueForValidKey()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "foo", 42 } };
        dict["foo"].Should().Be(42);
    }

    [Fact]
    public void IDictionary_IndexerGet_ShouldBeNull()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "foo", 42 } };
        var result = dict["bar"];
        result.Should().BeNull();
    }

    [Fact]
    public void IDictionary_IndexerGet_ShouldReturnNullForWrongKeyType()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "foo", 42 } };
        dict[123].Should().BeNull();
    }

    [Fact]
    public void IDictionary_IndexerGet_ShouldThrowForNullKey()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "foo", 42 } };
        Action act = () => { var _ = dict[null!]; };
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IDictionary_IndexerSet_ShouldSetValueForValidKeyAndValue()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        dict["foo"] = 123;
        dict["foo"].Should().Be(123);
    }

    [Fact]
    public void IDictionary_IndexerSet_ShouldThrowForNullKey()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        Action act = () => dict[null!] = 1;
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IDictionary_IndexerSet_ShouldThrowForWrongKeyType()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        Action act = () => dict[123] = 1; // int is not string
        act.Should().Throw<ArgumentException>()
            .WithMessage("*123*String*");
    }

    [Fact]
    public void IDictionary_IndexerSet_ShouldThrowForWrongValueType()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        Action act = () => dict["foo"] = "bar"; // string is not int
        act.Should().Throw<ArgumentException>()
            .WithMessage("*bar*Int32*");
    }

    [Fact]
    public void IDictionary_IndexerSet_ShouldThrowForNullValueOnValueType()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        Action act = () => dict["foo"] = null!;
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IDictionary_IndexerSet_ShouldAllowNullValueForReferenceType()
    {
        IDictionary dict = new ReadHeavyDictionary<string, string>();
        dict["foo"] = null;
        dict["foo"].Should().BeNull();
    }

    [Fact]
    public void IDictionary_IsFixedSize_ShouldBeFalse()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        dict.IsFixedSize.Should().BeFalse();
    }

    [Fact]
    public void IDictionary_IsReadOnly_ShouldBeFalse()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int>();
        dict.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void IDictionary_Keys_ShouldReturnAllKeys()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var keys = dict.Keys;
        keys.Should().BeEquivalentTo(new[] { "a", "b" });
    }

    [Fact]
    public void IDictionary_Values_ShouldReturnAllValues()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var values = dict.Values;
        values.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void IDictionary_GetEnumerator_ShouldEnumerateAllItems()
    {
        IDictionary dict = new ReadHeavyDictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var enumerator = dict.GetEnumerator();
        var items = new List<KeyValuePair<string, int>>();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current is DictionaryEntry de)
                items.Add(new KeyValuePair<string, int>((string)de.Key, (int)de.Value!));
            else if (enumerator.Current is KeyValuePair<string, int> kvp)
                items.Add(kvp);
        }
        items.Should().Contain(new KeyValuePair<string, int>("a", 1));
        items.Should().Contain(new KeyValuePair<string, int>("b", 2));
        items.Count.Should().Be(2);
    }

    [Fact]
    public void IndexerSet_ShouldAllowNull_ForNullableValueType()
    {
        var dict = new ReadHeavyDictionary<string, int?>();
        dict["foo"] = null;
        dict["foo"].Should().BeNull();
    }

    [Fact]
    public void IndexerSet_ShouldAllowNull_ForReferenceType()
    {
        var dict = new ReadHeavyDictionary<string, string>();
        dict["foo"] = null!;
        dict["foo"].Should().BeNull();
    }

    [Fact]
    public void Updating_Same_Key_Should_Work_Correctly()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict["key"] = 1;
        dict["key"] = 1;
        dict["key"].Should().Be(1);
        dict.Count.Should().Be(1);
    }

    [Fact]
    public void Updating_Same_Key_To_Different_Value_Should_Work_Correctly()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        dict["key"] = 1;
        dict["key"] = 2;
        dict["key"].Should().Be(2);
        dict.Count.Should().Be(1);
    }

    private class MyReadOnlyCollection<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        private readonly List<KeyValuePair<TKey, TValue>> _items;
        public MyReadOnlyCollection(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            _items = [.. items];
        }
        public int Count => _items.Count;
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }

    [Fact]
    public void AddRange_On_MyReadOnlyCollection_Should_Work_Correctly()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        var readOnlyCollection = new MyReadOnlyCollection<string, int>(new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 2)
        });

        dict.AddRange(readOnlyCollection);
        dict.Count.Should().Be(2);
    }

    [Fact]
    public void AddRange_On_IEnumerable_Should_Work_Correctly()
    {
        var dict = new ReadHeavyDictionary<string, int>();
        var iEnumerable = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 2)
        }.AsEnumerable();

        dict.AddRange(iEnumerable);
        dict.Count.Should().Be(2);
    }
}
