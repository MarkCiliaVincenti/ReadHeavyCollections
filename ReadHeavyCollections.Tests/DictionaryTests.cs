using Xunit;

namespace ReadHeavyCollections.Tests;

public class DictionaryTests
{
    [Fact]
    public void ReadTest()
    {
        ReadHeavyDictionary<int, int> readHeavyDictionary = new() { [0] = 1 };
        Assert.Equal(1, readHeavyDictionary[0]);
    }
}