using NUnit.Framework;

// Fixtures run concurrently; the tests inside a fixture stay sequential. The composition pipeline holds no shared
// mutable state (CsCheck already samples it on every logical CPU), the seeded fixtures carry no instance state
// across tests, and the worker count follows Environment.ProcessorCount (2 on the CI runner). A fixture that
// needs the whole process to itself opts out with [NonParallelizable].
[assembly: Parallelizable(ParallelScope.Fixtures)]
