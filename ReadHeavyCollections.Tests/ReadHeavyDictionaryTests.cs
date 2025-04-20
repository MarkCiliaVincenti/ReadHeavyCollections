using FluentAssertions;
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
        var dict = new ReadHeavyDictionary<string, int>
        {
            { "one", 1 }
        };
        dict["one"].Should().Be(1);
    }

    [Fact]
    public void Add_DuplicateKey_ShouldThrow()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            { "k", 1 }
        };
        FluentActions.Invoking(() => dict.Add("k", 2))
            .Should().Throw<ArgumentException>();
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
}