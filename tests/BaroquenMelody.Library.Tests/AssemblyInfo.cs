using NUnit.Framework;

// Fixtures run concurrently; the tests inside a fixture stay sequential unless the fixture opts into
// [Parallelizable(ParallelScope.All)] (the state-free seeded sweep fixtures do, so their long cases spread across
// the CI runner's four workers). The composition pipeline holds no shared mutable state (CsCheck already samples it
// on every logical CPU), the seeded fixtures carry no instance state across tests, and the worker count follows
// Environment.ProcessorCount (4 on the public-repository CI runner). A fixture that needs the whole process to
// itself opts out with [NonParallelizable].
[assembly: Parallelizable(ParallelScope.Fixtures)]
