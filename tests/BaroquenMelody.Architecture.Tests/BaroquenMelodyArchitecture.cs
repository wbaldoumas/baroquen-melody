using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using Assembly = System.Reflection.Assembly;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Loads every production and test assembly once per test run. Building an
///     <see cref="ArchUnitNET.Domain.Architecture"/> reads every type with Mono.Cecil, so it is cached in a
///     static field and shared by every fixture. Rules must scope by assembly (never by bare namespace
///     prefix alone) because the console app declares its own BaroquenMelody.Infrastructure.FileSystem
///     namespace that collides with the real Infrastructure project's prefix.
/// </summary>
internal static class BaroquenMelodyArchitecture
{
    public static readonly Assembly Library = Assembly.Load("BaroquenMelody.Library");

    public static readonly Assembly Infrastructure = Assembly.Load("BaroquenMelody.Infrastructure");

    public static readonly Assembly Components = Assembly.Load("BaroquenMelody.App.Components");

    public static readonly Assembly Console = Assembly.Load("baroquen-melody");

    public static readonly Assembly LibraryTests = Assembly.Load("BaroquenMelody.Library.Tests");

    public static readonly Assembly InfrastructureTests = Assembly.Load("BaroquenMelody.Infrastructure.Tests");

    public static readonly Assembly ComponentsTests = Assembly.Load("BaroquenMelody.App.Components.Tests");

    public static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Library, Infrastructure, Components, Console, LibraryTests, InfrastructureTests, ComponentsTests)
        .Build();
}
