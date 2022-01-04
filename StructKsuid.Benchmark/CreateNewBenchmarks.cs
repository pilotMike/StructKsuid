using BenchmarkDotNet.Attributes;

namespace StructKsuid.Benchmark;

[MemoryDiagnoser]
public class CreateNewBenchmarks
{
    [Benchmark(Baseline = true)]
    public KSUID.Ksuid KsuidGenerate() => KSUID.Ksuid.Generate();

    [Benchmark]
    public StructKsuid.Ksuid StructNewKsuid() => StructKsuid.Ksuid.NewKsuid();

    [Benchmark]
    public DotKsuid.Ksuid DotKsuidNewKsuid() => DotKsuid.Ksuid.NewKsuid();
}