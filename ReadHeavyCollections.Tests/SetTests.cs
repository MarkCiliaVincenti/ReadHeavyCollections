using Xunit;

namespace ReadHeavyCollections.Tests;

public class SetTests
{
    [Fact]
    public void ReadTest()
    {
        ReadHeavySet<int> readHeavySet = [1];
        Assert.Equal(1, readHeavySet.First());
    }
}