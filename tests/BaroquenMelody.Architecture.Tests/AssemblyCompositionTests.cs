using ArchUnitNET.Loader;
using NUnit.Framework;
using System.Diagnostics;

namespace BaroquenMelody.ArchitectureTests;

[TestFixture]
internal sealed class AssemblyCompositionTests
{
    [Test]
    public void Architecture_loads_every_production_and_test_assembly()
    {
        var architecture = BaroquenMelodyArchitecture.Architecture;

        foreach (var assembly in architecture.Assemblies)
        {
            var count = architecture.Types.Count(type => type.Assembly.Equals(assembly));
            TestContext.Out.WriteLine($"{assembly.Name}: {count} types");
        }

        Assert.That(architecture.Assemblies.Count(), Is.EqualTo(7));
    }

    [Test]
    public void Measure_architecture_build_time()
    {
        var stopwatch = Stopwatch.StartNew();

        var architecture = new ArchLoader()
            .LoadAssemblies(
                BaroquenMelodyArchitecture.Library,
                BaroquenMelodyArchitecture.Infrastructure,
                BaroquenMelodyArchitecture.Components,
                BaroquenMelodyArchitecture.Console,
                BaroquenMelodyArchitecture.LibraryTests,
                BaroquenMelodyArchitecture.InfrastructureTests,
                BaroquenMelodyArchitecture.ComponentsTests)
            .Build();

        stopwatch.Stop();

        TestContext.Out.WriteLine($"cold build of {architecture.Types.Count()} types took {stopwatch.ElapsedMilliseconds} ms");
    }
}
