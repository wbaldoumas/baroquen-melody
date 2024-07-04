using BaroquenMelody.Benchmarks.Compositions.Rules;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<AggregateCompositionRuleBenchmarks>(new DebugInProcessConfig());
