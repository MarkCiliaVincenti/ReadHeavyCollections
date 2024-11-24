using BenchmarkDotNet.Attributes;
using ReadHeavyCollections;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Benchmarks;

[MemoryDiagnoser]
public class SynchronousReadsIntegers
{
    [Params(10, 100, 1000, 10000, 100_000, 1_000_000)]
    public int Size;

    private int[] _keys;

    private Dictionary<int, int> _dictionary;
    private ConcurrentDictionary<int, int> _concurrentDictionary;
    private FrozenDictionary<int, int> _frozenDictionary;
    private ReadHeavyDictionary<int, int> _readHeavyDictionary;


    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(123);

        var uniqueKeys = new HashSet<int>(Size);
        for (var i = 0; i < Size; i++)
        {
            int key;
            do
            {
                key = random.Next();
            } while (uniqueKeys.Contains(key));

            uniqueKeys.Add(key);
        }

        _dictionary = uniqueKeys.Select((key, idx) => (key, idx)).ToDictionary(e => e.key, e => e.idx);
        _concurrentDictionary = new(_dictionary);
        _frozenDictionary = _dictionary.ToFrozenDictionary();
        _readHeavyDictionary = new(_dictionary);

        _keys = [.. uniqueKeys];
    }

    [Benchmark]
    public int Dictionary()
    {
        var latestValue = -1;

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
    public int ConcurrentDictionary()
    {
        var latestValue = -1;

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
    public int FrozenDictionary()
    {
        var latestValue = -1;

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
    public int ReadHeavyDictionary()
    {
        var latestValue = -1;

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