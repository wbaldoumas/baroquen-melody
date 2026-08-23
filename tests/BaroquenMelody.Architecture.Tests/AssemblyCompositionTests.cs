using NUnit.Framework;

namespace BaroquenMelody.ArchitectureTests;

[TestFixture]
internal sealed class AssemblyCompositionTests
{
    private static readonly string[] ExpectedAssemblies =
    [
        "BaroquenMelody.Library",
        "BaroquenMelody.Infrastructure",
        "BaroquenMelody.App.Components",
        "baroquen-melody",
        "BaroquenMelody.Benchmarks",
        "BaroquenMelody.Library.Tests",
        "BaroquenMelody.Infrastructure.Tests",
        "BaroquenMelody.App.Components.Tests",
    ];

    [Test]
    public void Architecture_loads_every_expected_assembly()
    {
        var architecture = BaroquenMelodyArchitecture.Architecture;

        foreach (var assembly in architecture.Assemblies)
        {
            var count = architecture.Types.Count(type => type.Assembly.Equals(assembly));
            TestContext.Out.WriteLine($"{assembly.Name}: {count} types");
        }

        Assert.That(architecture.Assemblies.Select(assembly => assembly.Name), Is.EquivalentTo(ExpectedAssemblies));
    }
}
