using NUnit.Framework;

// Fixtures run concurrently, but tests within a fixture stay sequential on one worker, so each fixture's
// bUnit TestContext (created in [SetUp] or field initializers) is never shared across workers. Fixtures
// must not share mutable static state.
[assembly: Parallelizable(ParallelScope.Fixtures)]
