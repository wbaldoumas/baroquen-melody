using NUnit.Framework;

// Fixtures run concurrently, but tests within a fixture stay sequential on one worker, so per-fixture
// instance state guarded by [SetUp] remains safe. Fixtures must not share mutable static state.
[assembly: Parallelizable(ParallelScope.Fixtures)]
