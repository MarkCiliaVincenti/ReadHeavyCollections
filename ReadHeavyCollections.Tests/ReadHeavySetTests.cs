using FluentAssertions;
using Xunit;

namespace ReadHeavyCollections.Tests;

public class ReadHeavySetTests
{
    [Fact]
    public void Add_ShouldReturnTrue_WhenNewItemAdded()
    {
        var set = new ReadHeavySet<int>();
        var added = set.Add(42);
        added.Should().BeTrue();
        set.Count.Should().Be(1);
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenItemExists()
    {
        var set = new ReadHeavySet<string>
        {
            "hello"
        };
        set.Add("hello").Should().BeFalse();
    }

    [Fact]
    public void Remove_ShouldReturnTrue_IfItemExisted()
    {
        var set = new ReadHeavySet<int> { 10 };
        set.Remove(10).Should().BeTrue();
        set.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_ShouldEmptyTheSet()
    {
        var set = new ReadHeavySet<int> { 1, 2, 3 };
        set.Clear();
        set.Count.Should().Be(0);
    }

    [Fact]
    public void Contains_ShouldReturnExpectedResult()
    {
        var set = new ReadHeavySet<string> { "a", "b" };
        set.Contains("a").Should().BeTrue();
        set.Contains("z").Should().BeFalse();
    }

    [Fact]
    public void Enumerator_ShouldIterateOverSnapshot()
    {
        var set = new ReadHeavySet<int>();
        for (int i = 0; i < 5; i++) set.Add(i);

        var snapshot = set.ToList();
        snapshot.Should().BeEquivalentTo([0, 1, 2, 3, 4]);
    }

    [Fact]
    public void Set_ShouldSupport_CustomComparer()
    {
        var set = new ReadHeavySet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HELLO"
        };
        set.Contains("hello").Should().BeTrue();
    }

    [Fact]
    public void ConcurrentReads_ShouldBeThreadSafe()
    {
        var set = new ReadHeavySet<int>(Enumerable.Range(1, 1000));
        Parallel.For(1, 1000, i => set.Contains(i).Should().BeTrue());
    }
}