using BenchmarkDotNet.Attributes;
using ReadHeavyCollections;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Benchmarks;

[MemoryDiagnoser]
public class SynchronousReadsStrings
{
    [Params(10, 100, 1000, 10000, 100_000, 1_000_000)]
    public int Size;

    private string[] _keys;

    private Dictionary<string, string> _dictionary;
    private ConcurrentDictionary<string, string> _concurrentDictionary;
    private FrozenDictionary<string, string> _frozenDictionary;
    private ReadHeavyDictionary<string, string> _readHeavyDictionary;


    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(123);

        var uniqueKeys = new HashSet<string>(Size);
        for (var i = 0; i < Size; i++)
        {
            string key;
            do
            {
                key = random.Next().ToString();
            } while (uniqueKeys.Contains(key));

            uniqueKeys.Add(key);
        }

        _dictionary = uniqueKeys.Select((key, idx) => (key, idx)).ToDictionary(e => e.key, e => e.idx.ToString());
        _concurrentDictionary = new(_dictionary);
        _frozenDictionary = _dictionary.ToFrozenDictionary();
        _readHeavyDictionary = new(_dictionary);

        _keys = [.. uniqueKeys];
    }

    [Benchmark]
    public string Dictionary()
    {
        var latestValue = "-1";

        foreach (var key in _keys)
        {
            if (_dictionary.TryGetValue(key, out var value))
            {
                latestValue = value;
            }
        }

        return latestValue;
    }

    [Benchmark]
    public string ConcurrentDictionary()
    {
        var latestValue = "-1";

        foreach (var key in _keys)
        {
            if (_concurrentDictionary.TryGetValue(key, out var value))
            {
                latestValue = value;
            }
        }

        return latestValue;
    }

    [Benchmark]
    public string FrozenDictionary()
    {
        var latestValue = "-1";

        foreach (var key in _keys)
        {
            if (_frozenDictionary.TryGetValue(key, out var value))
            {
                latestValue = value;
            }
        }

        return latestValue;
    }

    [Benchmark(Baseline = true)]
    public string ReadHeavyDictionary()
    {
        var latestValue = "-1";

        foreach (var key in _keys)
        {
            if (_readHeavyDictionary.TryGetValue(key, out var value))
            {
                latestValue = value;
            }
        }

        return latestValue;
    }
}