// using System.Runtime.InteropServices;
// using BenchmarkDotNet.Attributes;
// using KSUID;
//
// namespace StructKsuid.Benchmark;
//
// [MemoryDiagnoser]
// [ShortRunJob]
// public class IntArrayBenchmarks
// {
//     public static int[][] inputs { get; } = Enumerable.Range(0, 1000).Select(_ => Ksuid.Generate().ToByteArray())
//         .Select(bytes =>
//         {
//             int[] ints = new int[bytes.Length / 4];
//             bytes.AsSpan().CopyTo(MemoryMarshal.Cast<int, byte>(ints));
//             return ints;
//         }).ToArray();
//
//     [Benchmark(Baseline = true)]
//     [Arguments(1)]
//     [Arguments(10)]
//     [Arguments(100)]
//     [Arguments(1000)]
//     public void Original(int timesToRun)
//     {
//         for (int i = 0; i < timesToRun; i++)
//         {
//             var val = ReferenceBase62.BaseConvert(inputs[i]);
//         }
//     }
//
//     [Benchmark]
//     [Arguments(1)]
//     [Arguments(10)]
//     [Arguments(100)]
//     [Arguments(1000)]
//     public void Reworked(int timesToRun)
//     {
//         for (int i = 0; i < timesToRun; i++)
//         {
//             var val = ReferenceBase62.Reworked(inputs[i]);
//         }
//     }
// }