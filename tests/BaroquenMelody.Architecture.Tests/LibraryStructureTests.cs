using ArchUnitNET.Domain;
using ArchUnitNET.NUnit;
using NUnit.Framework;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 2: structural conventions inside the Library. Internal Library types are addressed by full-name
///     strings (the test project has no InternalsVisibleTo grant); a stale string matches zero types and
///     the rule then FAILS thanks to ArchUnitNET's require-positive-results default, so typos cannot rot.
/// </summary>
[TestFixture]
internal sealed class LibraryStructureTests
{
    private static readonly Architecture Architecture = BaroquenMelodyArchitecture.Architecture;

    [Test]
    public void Composition_rules_reside_in_Rules_and_are_internal_sealed()
    {
        Classes()
            .That()
            .ImplementInterface(GetInterface("BaroquenMelody.Library.Rules.ICompositionRule"))
            .Should()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Rules(\..+)?$")
            .AndShould()
            .BeInternal()
            .AndShould()
            .BeSealed()
            .Check(Architecture);
    }

    [Test]
    public void Melodic_composition_rules_reside_in_Rules_and_are_internal_sealed()
    {
        Classes()
            .That()
            .ImplementInterface(GetInterface("BaroquenMelody.Library.Rules.Melodic.IMelodicCompositionRule"))
            .Should()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Rules(\..+)?$")
            .AndShould()
            .BeInternal()
            .AndShould()
            .BeSealed()
            .Check(Architecture);
    }

    [Test]
    public void Ornamentation_types_are_internal_except_the_public_enums()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Ornamentation")
            .And()
            .AreNotEnums()
            .Should()
            .BeInternal()
            .Check(Architecture);
    }

    [Test]
    public void Store_states_are_public_sealed_records_named_State()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Store\.State$")
            .Should()
            .BeRecord()
            .AndShould()
            .BeSealed()
            .AndShould()
            .BePublic()
            .AndShould()
            .HaveNameEndingWith("State")
            .Check(Architecture);
    }

    [Test]
    public void Store_actions_are_public_sealed_records()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Store\.Actions$")
            .Should()
            .BeRecord()
            .AndShould()
            .BeSealed()
            .AndShould()
            .BePublic()
            .Check(Architecture);
    }

    [Test]
    public void Store_reducers_are_static_classes_named_Reducers()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Store\.Reducers$")
            .Should()
            .HaveNameEndingWith("Reducers")
            .AndShould()
            .BeSealed()
            .AndShould()
            .BeAbstract()
            .Because("sealed+abstract in IL is exactly a C# static class")
            .Check(Architecture);
    }

    [Test]
    public void Store_effects_are_sealed_classes_named_Effects()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Store\.Effects$")
            .Should()
            .HaveNameEndingWith("Effects")
            .AndShould()
            .BeSealed()
            .Check(Architecture);
    }

    [Test]
    public void Library_enums_reside_in_an_Enums_namespace()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .AreEnums()
            .Should()
            .ResideInNamespaceMatching(@"\.Enums$")
            .Check(Architecture);
    }

    [Test]
    public void Interfaces_are_prefixed_with_I()
    {
        Interfaces()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library, BaroquenMelodyArchitecture.Infrastructure, BaroquenMelodyArchitecture.Components)
            .Should()
            .HaveNameStartingWith("I")
            .Check(Architecture);
    }

    [Test]
    public void Library_classes_are_sealed_static_or_compiler_generated()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .AreNotAssignableTo(typeof(System.Text.Json.Serialization.JsonSerializerContext))
            .Should()
            .BeSealed()
            .Because("the Library has no inheritance hierarchies: every concrete class is sealed, every static class is sealed+abstract in IL; only the source-generated JsonSerializerContext is exempt")
            .Check(Architecture);
    }

    [Test]
    public void The_Library_never_touches_System_Random_directly()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullName("System.Random")
            .Because("randomness flows through Infrastructure.Random's IRandomProvider seam so seeded runs stay reproducible")
            .Check(Architecture);
    }

    [Test]
    public void Only_the_two_random_seams_construct_System_Random()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Infrastructure)
            .And()
            .DoNotHaveName("ThreadLocalRandom")
            .And()
            .DoNotHaveName("SeededRandomProvider")
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullName("System.Random")
            .Check(Architecture);
    }

    [Test]
    public void System_Console_is_used_only_by_the_console_app()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library, BaroquenMelodyArchitecture.Infrastructure, BaroquenMelodyArchitecture.Components)
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullName("System.Console")
            .Check(Architecture);
    }

    [Test]
    public void Static_file_system_APIs_stay_out_of_the_Library()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullNameMatching(@"^System\.IO\.(File|Directory|FileStream|StreamWriter|StreamReader)$")
            .Because("Library disk access goes through System.IO.Abstractions so it stays testable and AOT-friendly; System.IO.Path is pure string manipulation and stays allowed")
            .Check(Architecture);
    }

    [Test]
    public void Only_the_configurator_wires_concrete_pipeline_composers()
    {
        string[] concreteComposers = ["ChordComposer", "ThemeComposer", "EndingComposer", "GroundBassComposer", "Composer", "MidiFileComposer"];
        string[] allowedDependents = ["BaroquenMelodyComposerConfigurator", .. concreteComposers];

        var offenders = Architecture.Types
            .Where(type => type.Assembly.Equals(Architecture.Assemblies.First(assembly => string.Equals(assembly.Name, BaroquenMelodyArchitecture.Library.GetName().Name, StringComparison.Ordinal))))
            .Where(type => !allowedDependents.Contains(type.Name, StringComparer.Ordinal))
            .Where(type => type.Dependencies.Any(dependency =>
                concreteComposers.Contains(dependency.Target.Name, StringComparer.Ordinal)
                && !dependency.Target.Equals(type)))
            .Select(type => type.FullName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        Assert.That(offenders, Is.Empty, $"only the configurator may construct pipeline composers, but found: {string.Join(", ", offenders)}");
    }

    [Test]
    public void Configuration_types_are_public_sealed_records()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Configurations$")
            .Should()
            .BeRecord()
            .AndShould()
            .BeSealed()
            .AndShould()
            .BePublic()
            .Because("the configuration model is the UI's data contract and relies on record with-clone semantics")
            .Check(Architecture);
    }

    [Test]
    public void Midi_output_ports_are_implemented_only_by_hosts()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .Should()
            .NotImplementAnyInterfacesThat()
            .HaveFullNameMatching(@"^BaroquenMelody\.Library\.Midi\.(IMidiLauncher|IMidiSaver)$")
            .Because("launching and saving MIDI are host concerns: the console app and the MAUI host provide the adapters")
            .Check(Architecture);
    }

    [Test]
    public void The_policy_engine_is_confined_to_the_ornamentation_and_dynamics_engines()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .DoNotResideInNamespaceMatching(@"^BaroquenMelody\.Library$|^BaroquenMelody\.Library\.(Ornamentation|Dynamics)(\..+)?$")
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^Atrea\.PolicyEngine")
            .Check(Architecture);
    }

    private static Interface GetInterface(string fullName)
    {
        return Architecture.Interfaces.First(candidate => string.Equals(candidate.FullName, fullName, StringComparison.Ordinal));
    }
}
