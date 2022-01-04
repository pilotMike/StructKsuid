// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using StructKsuid.Benchmark;

BenchmarkRunner.Run<ParseBenchmarks>();

Console.ReadLine();