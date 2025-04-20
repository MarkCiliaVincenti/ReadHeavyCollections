using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
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
}
