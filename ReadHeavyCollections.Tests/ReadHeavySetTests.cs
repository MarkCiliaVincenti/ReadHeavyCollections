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

    [Fact]
    public void Create_WithReadOnlySpan_ShouldReturnSetWithValues()
    {
        var span = new ReadOnlySpan<int>(new[] { 1, 2, 3 });
        var set = ReadHeavySet.Create(span);
        set.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Create_WithReadOnlySpanAndComparer_ShouldReturnSetWithComparer()
    {
        var span = new ReadOnlySpan<string>(new[] { "A", "b" });
        var set = ReadHeavySet.Create(StringComparer.OrdinalIgnoreCase, span);
        set.Contains("a").Should().BeTrue();
        set.Contains("B").Should().BeTrue();
    }

    [Fact]
    public void ToReadHeavySet_WithEnumerable_ShouldReturnSet()
    {
        var source = new List<int> { 1, 2, 3 };
        var set = source.ToReadHeavySet();
        set.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void ToReadHeavySet_WithEnumerableAndComparer_ShouldReturnSetWithComparer()
    {
        var source = new List<string> { "A", "b" };
        var set = source.ToReadHeavySet(StringComparer.OrdinalIgnoreCase);
        set.Contains("a").Should().BeTrue();
        set.Contains("B").Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithISet_ShouldCopyElements()
    {
        ISet<int> baseSet = new HashSet<int> { 1, 2, 3 };
        var set = new ReadHeavySet<int>(baseSet);
        set.Should().BeEquivalentTo(baseSet);
    }

    [Fact]
    public void Constructor_WithISetAndComparer_ShouldCopyElementsAndUseComparer()
    {
        ISet<string> baseSet = new HashSet<string> { "A", "b" };
        var set = new ReadHeavySet<string>(baseSet, StringComparer.OrdinalIgnoreCase);
        set.Contains("a").Should().BeTrue();
        set.Contains("B").Should().BeTrue();
    }

    [Fact]
    public void ICollectionT_IsReadOnly_ShouldBeFalse()
    {
        ICollection<int> set = new ReadHeavySet<int>();
        set.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void ICollectionT_Add_ShouldAddItem()
    {
        ICollection<int> set = new ReadHeavySet<int>();
        set.Add(123);
        set.Should().Contain(123);
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEnumerator()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2 });
        var enumerator = set.GetEnumerator();
        var items = new List<int>();
        while (enumerator.MoveNext())
            items.Add(enumerator.Current);
        items.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void ISetT_Add_ShouldReturnTrueIfAdded()
    {
        ISet<int> set = new ReadHeavySet<int>();
        set.Add(77).Should().BeTrue();
        set.Add(77).Should().BeFalse();
    }

    [Fact]
    public void IsProperSubsetOf_ShouldReturnExpected()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2 });
        set.IsProperSubsetOf(new[] { 1, 2, 3 }).Should().BeTrue();
        set.IsProperSubsetOf(new[] { 1, 2 }).Should().BeFalse();
    }

    [Fact]
    public void IsProperSupersetOf_ShouldReturnExpected()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2, 3 });
        set.IsProperSupersetOf(new[] { 1, 2 }).Should().BeTrue();
        set.IsProperSupersetOf(new[] { 1, 2, 3 }).Should().BeFalse();
    }

    [Fact]
    public void IsSubsetOf_ShouldReturnExpected()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2 });
        set.IsSubsetOf(new[] { 1, 2, 3 }).Should().BeTrue();
        set.IsSubsetOf(new[] { 1, 2 }).Should().BeTrue();
        set.IsSubsetOf(new[] { 2, 3 }).Should().BeFalse();
    }

    [Fact]
    public void IsSupersetOf_ShouldReturnExpected()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2, 3 });
        set.IsSupersetOf(new[] { 1, 2 }).Should().BeTrue();
        set.IsSupersetOf(new[] { 1, 2, 3 }).Should().BeTrue();
        set.IsSupersetOf(new[] { 4 }).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_ShouldReturnExpected()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2, 3 });
        set.Overlaps(new[] { 2, 99 }).Should().BeTrue();
        set.Overlaps(new[] { 99, 100 }).Should().BeFalse();
    }

    [Fact]
    public void SetEquals_ShouldReturnExpected()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2, 3 });
        set.SetEquals(new[] { 3, 2, 1 }).Should().BeTrue();
        set.SetEquals(new[] { 1, 2 }).Should().BeFalse();
    }

    [Fact]
    public void ICollection_IsSynchronized_ShouldBeTrue()
    {
        System.Collections.ICollection set = new ReadHeavySet<int>();
        set.IsSynchronized.Should().BeTrue();
    }

    [Fact]
    public void ICollection_SyncRoot_ShouldReturnThis()
    {
        var set = new ReadHeavySet<int>();
        ((System.Collections.ICollection)set).SyncRoot.Should().BeSameAs(set);
    }

    [Fact]
    public void ICollection_CopyTo_ShouldCopyItems()
    {
        ICollection<int> set = new ReadHeavySet<int>(new[] { 1, 2 });
        var array = new int[2];
        set.CopyTo(array, 0);
        array.Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public void OnDeserialization_ShouldNotThrow()
    {
        var set = new ReadHeavySet<int>(new[] { 1, 2 });
        set.OnDeserialization(null);
        set.Should().Contain(1);
        set.Should().Contain(2);
    }
}