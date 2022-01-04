using BenchmarkDotNet.Attributes;

namespace StructKsuid.Benchmark;

[MemoryDiagnoser]
public class CreateNewBenchmarks
{
    [Benchmark(Baseline = true)]
    public KSUID.Ksuid KsuidGenerate() => KSUID.Ksuid.Generate();

    [Benchmark]
    public StructKsuid.Ksuid StructNextKsuid() => StructKsuid.Ksuid.NextKsuid();
    
    [Benchmark]
    public StructKsuid.Ksuid StructRandomKsuid() => StructKsuid.Ksuid.RandomKsuid();

    [Benchmark]
    public DotKsuid.Ksuid DotKsuidNewKsuid() => DotKsuid.Ksuid.NewKsuid();
}