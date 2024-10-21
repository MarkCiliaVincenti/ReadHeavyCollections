﻿using BenchmarkDotNet.Attributes;
using ReadHeavyCollections;
using System.Collections.Concurrent;

[MemoryDiagnoser]
public class SynchronousRemove
{
    [Params(10, 100, 1000, 10000, 100000)]
    public int Size;

    private Dictionary<int, int> _dictionary;
    private ConcurrentDictionary<int, int> _concurrentDictionary;
    private ReadHeavyDictionary<int, int> _readHeavyDictionary;    

    [GlobalSetup]
    public void Setup()
    {
        var uniqueKeys = new HashSet<int>(Size);
        for (var i = 0; i < Size - 1; i++)
        {
            int key;
            do
            {
                key = Random.Shared.Next();
            } while (uniqueKeys.Contains(key));

            uniqueKeys.Add(key);
        }

        _dictionary = uniqueKeys.Select((key, idx) => (key, idx)).ToDictionary(e => e.key, e => e.idx);
        _concurrentDictionary = new(_dictionary);
        _readHeavyDictionary = new(_dictionary);
    }

    [IterationSetup]
    public void Cleanup()
    {
        _dictionary[-1] = -1;
        _concurrentDictionary[-1] = -1;
        _readHeavyDictionary[-1] = -1;
    }

    [Benchmark]
    public void Dictionary()
    {
        _dictionary.Remove(-1);
    }

    [Benchmark]
    public void ConcurrentDictionary()
    {
        _concurrentDictionary.Remove(-1, out _);
    }

    [Benchmark(Baseline = true)]
    public void ReadHeavyDictionary()
    {
        _readHeavyDictionary.Remove(-1);
    }
}