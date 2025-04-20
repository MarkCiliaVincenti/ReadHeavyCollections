using FluentAssertions;
using Xunit;

namespace ReadHeavyCollections.Tests;

public class ReadHeavyDictionaryTests
{
    [Fact]
    public void Add_And_TryGetValue_Works()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            { "a", 1 }
        };

        dict.Count.Should().Be(1);
        dict.TryGetValue("a", out var value).Should().BeTrue();
        value.Should().Be(1);
    }

    [Fact]
    public void Add_Throws_On_Duplicate_Key()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            { "key", 42 }
        };
        var act = () => dict.Add("key", 100);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Indexer_Get_And_Set_Works()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            ["x"] = 10
        };
        dict["x"].Should().Be(10);

        dict["x"] = 20;
        dict["x"].Should().Be(20);
    }

    [Fact]
    public void Remove_Works_Correctly()
    {
        var dict = new ReadHeavyDictionary<string, int>
        {
            { "k", 5 }
        };
        dict.Remove("k").Should().BeTrue();
        dict.ContainsKey("k").Should().BeFalse();
    }

    [Fact]
    public void ContainsKey_Returns_Correctly()
    {
        var dict = new ReadHeavyDictionary<int, string>
        {
            { 1, "one" }
        };
        dict.ContainsKey(1).Should().BeTrue();
        dict.ContainsKey(99).Should().BeFalse();
    }

    [Fact]
    public void Clear_Removes_All()
    {
        var dict = new ReadHeavyDictionary<int, string>
        {
            { 1, "a" },
            { 2, "b" }
        };
        dict.Clear();
        dict.Count.Should().Be(0);
    }

    [Fact]
    public void Can_Enumerate_All_Items()
    {
        var dict = new ReadHeavyDictionary<int, string>
        {
            [1] = "a",
            [2] = "b"
        };

        var keys = dict.Select(kv => kv.Key).ToList();
        keys.Should().Contain(new[] { 1, 2 });
    }

    [Fact]
    public void Concurrent_Reads_Are_Thread_Safe()
    {
        var dict = new ReadHeavyDictionary<int, string>();
        for (int i = 0; i < 1000; i++)
            dict[i] = i.ToString();

        Parallel.For(0, 1000, i =>
        {
            dict[i].Should().Be(i.ToString());
        });
    }

    [Fact]
    public void ReadTest()
    {
        ReadHeavyDictionary<int, int> readHeavyDictionary = new() { [0] = 1 };
        Assert.Equal(1, readHeavyDictionary[0]);
    }
}