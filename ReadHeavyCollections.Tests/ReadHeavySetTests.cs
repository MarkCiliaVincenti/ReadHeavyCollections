using FluentAssertions;
using Xunit;

namespace ReadHeavyCollections.Tests;

public class ReadHeavySetTests
{
    [Fact]
    public void ReadTest()
    {
        ReadHeavySet<int> readHeavySet = [1];
        Assert.Equal(1, readHeavySet.First());
    }

    [Fact]
    public void Add_And_Contains_Works()
    {
        var set = new ReadHeavySet<string>();
        set.Add("apple").Should().BeTrue();
        set.Contains("apple").Should().BeTrue();
    }

    [Fact]
    public void Add_Returns_False_On_Duplicate()
    {
        var set = new ReadHeavySet<int>();
        set.Add(1).Should().BeTrue();
        set.Add(1).Should().BeFalse();
    }

    [Fact]
    public void Remove_Works_Correctly()
    {
        var set = new ReadHeavySet<int>
        {
            42
        };
        set.Remove(42).Should().BeTrue();
        set.Contains(42).Should().BeFalse();
    }

    [Fact]
    public void Clear_Removes_All()
    {
        var set = new ReadHeavySet<string>
        {
            "x",
            "y"
        };
        set.Clear();
        set.Count.Should().Be(0);
    }

    [Fact]
    public void Can_Enumerate_Items()
    {
        var set = new ReadHeavySet<int>
        {
            1,
            2,
            3
        };

        var values = set.ToList();
        values.Should().Contain([1, 2, 3]);
    }

    [Fact]
    public void Concurrent_Reads_Are_Thread_Safe()
    {
        var set = new ReadHeavySet<int>(Enumerable.Range(0, 1000));
        Parallel.For(0, 1000, i =>
        {
            set.Contains(i).Should().BeTrue();
        });
    }
}