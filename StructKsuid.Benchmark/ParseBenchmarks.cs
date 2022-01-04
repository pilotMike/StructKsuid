using BenchmarkDotNet.Attributes;

namespace StructKsuid.Benchmark;

[MemoryDiagnoser]
public class ParseBenchmarks
{
    // public static string[] Values { get; } = Enumerable.Range(0, 10).Select(_ => new KSUID.Ksuid().ToString()).ToArray();
    //
    // [ParamsSource(nameof(Values))]
    // public string ID { get; set; }
    private readonly string ID = new KSUID.Ksuid().ToString();

    [Benchmark(Baseline = true)]
    public KSUID.Ksuid KsuidParse() => KSUID.Ksuid.FromString(ID);

    [Benchmark]
    public DotKsuid.Ksuid DotKsuidParse() => DotKsuid.Ksuid.Parse(ID);

    [Benchmark]
    public StructKsuid.Ksuid StructKsuidParse() => StructKsuid.Ksuid.Parse(ID);
}