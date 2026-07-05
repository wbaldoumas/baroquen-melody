using BaroquenMelody.Benchmarks.Compositions;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(BenchmarkData).Assembly).Run(args);
