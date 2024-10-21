using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using ReadHeavyCollections;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Benchmarks;

[Config(typeof(MemoryConfig))]
[MemoryDiagnoser]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
public class Benchmarks
{
    private class MemoryConfig : ManualConfig
    {
        public MemoryConfig()
        {
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    [Params(2_000, 100_000)] public int NumberOfItems { get; set; }
    //[Params(200, 10_000)] public int NumberOfAdds { get; set; }
    //[Params(200, 10_000)] public int NumberOfRemoves { get; set; }
    [Params(2_000, 100_000)] public int NumberOfReads { get; set; }

    public FrozenDictionary<int, int>? FrozenDictionary { get; set; }
    public Dictionary<int, int>? Dictionary { get; set; }
    public ConcurrentDictionary<int, int>? ConcurrentDictionary { get; set; }
    public ReadHeavyDictionary<int, int>? ReadHeavyDictionary { get; set; }

    private async Task RunTests(ParallelQuery<Task> tasks)
    {
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public ParallelQuery<Task>? FrozenDictionaryTasks { get; set; }

    [IterationSetup(Target = nameof(FrozenDictionaryRead))]
    public void SetupFrozenDictionary()
    {
        FrozenDictionary = Enumerable.Range(0, NumberOfItems - 1).ToFrozenDictionary(x => x, x => x);
        FrozenDictionaryTasks = Enumerable.Range(0, NumberOfReads - 1).Select(async i =>
        {
            if (!FrozenDictionary.TryGetValue(i % (NumberOfItems - 1), out var result) || result != i % (NumberOfItems - 1))
            {
                throw new Exception();
            }
            await Task.Yield();
        }).AsParallel();
    }

    [IterationCleanup(Target = nameof(FrozenDictionaryRead))]
    public void CleanupFrozenDictionary()
    {
        FrozenDictionary = null;
        FrozenDictionaryTasks = null;
    }

    [Benchmark(Description = "FrozenDictionary Reads")]
    public async Task FrozenDictionaryRead()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        await RunTests(FrozenDictionaryTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
    }


    public ParallelQuery<Task>? DictionaryTasks { get; set; }

    [IterationSetup(Target = nameof(DictionaryRead))]
    public void SetupDictionary()
    {
        Dictionary = Enumerable.Range(0, NumberOfItems - 1).ToDictionary(x => x, x => x);
        DictionaryTasks = Enumerable.Range(0, NumberOfReads - 1).Select(async i =>
        {
            if (!Dictionary.TryGetValue(i % (NumberOfItems - 1), out var result) || result != i % (NumberOfItems - 1))
            {
                throw new Exception();
            }
            await Task.Yield();
        }).AsParallel();
    }

    [IterationCleanup(Target = nameof(DictionaryRead))]
    public void CleanupDictionary()
    {
        Dictionary = null;
        DictionaryTasks = null;
    }

    [Benchmark(Description = "Dictionary Reads")]
    public async Task DictionaryRead()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        await RunTests(DictionaryTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
    }


    public ParallelQuery<Task>? ConcurrentDictionaryTasks { get; set; }

    [IterationSetup(Target = nameof(ConcurrentDictionaryRead))]
    public void SetupConcurrentDictionary()
    {
        ConcurrentDictionary = new(Enumerable.Range(0, NumberOfItems - 1).ToDictionary(x => x, x => x));
        ConcurrentDictionaryTasks = Enumerable.Range(0, NumberOfReads - 1).Select(async i =>
        {
            if (!ConcurrentDictionary.TryGetValue(i % (NumberOfItems - 1), out var result) || result != i % (NumberOfItems - 1))
            {
                throw new Exception();
            }
            await Task.Yield();
        }).AsParallel();
    }

    [IterationCleanup(Target = nameof(ConcurrentDictionaryRead))]
    public void CleanupConcurrentDictionary()
    {
        ConcurrentDictionary = null;
        ConcurrentDictionaryTasks = null;
    }

    [Benchmark(Description = "ConcurrentDictionary Reads")]
    public async Task ConcurrentDictionaryRead()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        await RunTests(ConcurrentDictionaryTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
    }

    public ParallelQuery<Task>? ReadHeavyDictionaryTasks { get; set; }

    [IterationSetup(Target = nameof(ReadHeavyDictionaryRead))]
    public void SetupReadHeavyDictionary()
    {
        ReadHeavyDictionary = new(Enumerable.Range(0, NumberOfItems - 1).ToDictionary(x => x, x => x));
        ReadHeavyDictionaryTasks = Enumerable.Range(0, NumberOfReads - 1).Select(async i =>
        {
            if (!ReadHeavyDictionary.TryGetValue(i % (NumberOfItems - 1), out var result) || result != i % (NumberOfItems - 1))
            {
                throw new Exception();
            }
            await Task.Yield();
        }).AsParallel();
    }

    [IterationCleanup(Target = nameof(ReadHeavyDictionaryRead))]
    public void CleanupReadHeavyDictionary()
    {
        ReadHeavyDictionary = null;
        ReadHeavyDictionaryTasks = null;
    }

    [Benchmark(Baseline = true, Description = "ReadHeavyDictionary Reads")]
    public async Task ReadHeavyDictionaryRead()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        await RunTests(ReadHeavyDictionaryTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
    }
}