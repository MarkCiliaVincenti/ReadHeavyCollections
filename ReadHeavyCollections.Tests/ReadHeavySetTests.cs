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

    [Fact]
    public void AddRange_ShouldAddMultipleItems()
    {
        var set = new ReadHeavySet<int>();
        set.AddRange(new[] { 1, 2, 3 });
        set.Count.Should().Be(3);
        set.Contains(2).Should().BeTrue();
    }

    [Fact]
    public void Set_ExceptWith_ShouldRemoveItems()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2, 3, 4 });
        set.ExceptWith(new[] { 2, 3 });
        set.Should().BeEquivalentTo(new[] { 1, 4 });
    }

    [Fact]
    public void Set_IntersectWith_ShouldRetainCommonItems()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2, 3 });
        set.IntersectWith(new[] { 2, 3, 4 });
        set.Should().BeEquivalentTo(new[] { 2, 3 });
    }

    [Fact]
    public void Set_UnionWith_ShouldAddAllItems()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2 });
        set.UnionWith(new[] { 2, 3, 4 });
        set.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });
    }

    [Fact]
    public void Set_SymmetricExceptWith_ShouldWork()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2, 3 });
        set.SymmetricExceptWith(new[] { 2, 3, 4 });
        set.Should().BeEquivalentTo(new[] { 1, 4 });
    }

    [Fact]
    public void Set_CopyTo_ShouldCopyItems()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2 });
        var array = new int[2];
        set.CopyTo(array, 0);
        array.Should().BeEquivalentTo(new[] { 1, 2 });
    }
}