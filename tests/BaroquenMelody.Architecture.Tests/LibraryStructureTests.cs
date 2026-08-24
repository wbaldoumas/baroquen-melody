using ArchUnitNET.Domain;
using ArchUnitNET.NUnit;
using NUnit.Framework;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 2: structural conventions inside the Library. Internal Library interfaces are resolved to domain
///     objects from the cached Architecture (0.13 removed the string overloads), so a stale name throws at
///     lookup time instead of silently matching nothing.
/// </summary>
[TestFixture]
internal sealed class LibraryStructureTests
{
    private static readonly Architecture Architecture = BaroquenMelodyArchitecture.Architecture;

    private static readonly string[] ConcreteComposers =
    [
        "BaroquenMelody.Library.Composers.ChordComposer",
        "BaroquenMelody.Library.Composers.ThemeComposer",
        "BaroquenMelody.Library.Composers.EndingComposer",
        "BaroquenMelody.Library.Composers.GroundBassComposer",
        "BaroquenMelody.Library.Composers.Composer",
        "BaroquenMelody.Library.Composers.MidiFileComposer",
    ];

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
            .Because("rules are selected through CompositionRuleFactory and never exported or subclassed")
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
            .Because("melodic rules reach the walk only through MelodicCompositionRuleAdapter and stay inside Rules")
            .Check(Architecture);
    }

    [Test]
    public void Ornamentation_types_are_not_public_except_the_enums()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Ornamentation")
            .And()
            .AreNotEnums()
            .Should()
            .NotBePublic()
            .Because("the ornamentation engine is an implementation detail; only OrnamentationType is part of the configuration contract")
            .Check(Architecture);
    }

    [Test]
    public void Store_states_are_public_sealed_feature_state_records_named_State()
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
            .AndShould()
            .HaveAnyAttributes(typeof(Fluxor.FeatureStateAttribute))
            .Because("Fluxor discovers feature state by attribute and the UI binds IState<T> to these records")
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
            .Because("actions are immutable messages dispatched by the UI and the composers alike")
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
            .Because("reducers are pure static [ReducerMethod] functions; sealed+abstract in IL is exactly a C# static class")
            .Check(Architecture);
    }

    [Test]
    public void Store_effects_are_public_sealed_classes_named_Effects()
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
            .AndShould()
            .BePublic()
            .Because("Fluxor instantiates effect classes through DI; they are the only stateful Store types")
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
            .Because("19 of 21 Library enums already live in a *.Enums sub-namespace; the convention is real")
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
            .Because("the editorconfig naming rule is only a suggestion; this makes it a gate")
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
            .Because("ThreadLocalRandom and SeededRandomProvider are the only sanctioned constructors of System.Random")
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
            .Because("logging goes through Infrastructure.Logging.Log; only the console harness writes to the console")
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
            .HaveFullNameMatching(@"^System\.IO\.(File|Directory|DirectoryInfo|FileStream|StreamWriter|StreamReader)$")
            .Because("Library disk access goes through System.IO.Abstractions so it stays testable and AOT-friendly; System.IO.Path is pure string manipulation and stays allowed; FileInfo is used for metadata by the persistence service today and is a documented follow-up, not part of this rule yet")
            .Check(Architecture);
    }

    [Test]
    public void Only_the_configurator_wires_concrete_pipeline_composers()
    {
        string[] allowedDependents = ["BaroquenMelody.Library.BaroquenMelodyComposerConfigurator", .. ConcreteComposers];

        var offenders = Architecture.Types
            .Where(type => string.Equals(type.Assembly.Name, "BaroquenMelody.Library", StringComparison.Ordinal))
            .Where(type => !allowedDependents.Contains(type.FullName, StringComparer.Ordinal))
            .Where(type => type.Dependencies.Any(dependency =>
                ConcreteComposers.Contains(dependency.Target.FullName, StringComparer.Ordinal)
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
            .DoNotResideInNamespaceMatching(@"^BaroquenMelody\.Library\.(Ornamentation|Dynamics)(\..+)?$")
            .And()
            .DoNotHaveFullName("BaroquenMelody.Library.BaroquenMelodyComposerConfigurator")
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^Atrea\.PolicyEngine")
            .Because("the policy engine is the ornamentation and dynamics engines' implementation choice; only the configurator that builds them may see it")
            .Check(Architecture);
    }

    private static Interface GetInterface(string fullName)
    {
        return Architecture.Interfaces.First(candidate => string.Equals(candidate.FullName, fullName, StringComparison.Ordinal));
    }
}
