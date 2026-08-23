using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Slices;
using NUnit.Framework;
using System.Reflection;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Research probes: dumps the installed ArchUnitNET fluent API surface and runtime behaviors
///     so the architecture-rule catalog can be written against verified 0.13.4 facts.
/// </summary>
[TestFixture]
internal sealed class ApiSurfaceDumpTests
{
    [Test]
    public void Dump_fluent_api_method_names()
    {
        var assembly = typeof(ArchRuleDefinition).Assembly;

        var interesting = new[]
        {
            "TypesThat", "TypesShould", "ClassesThat", "ClassesShould", "ObjectsThat", "ObjectsShould",
            "MembersThat", "MembersShould", "MethodMembersThat", "MethodMembersShould",
            "SlicesShould", "GivenSlices", "SliceRuleDefinition", "ArchRuleDefinition", "ArchRule",
        };

        foreach (var type in assembly.GetTypes()
                     .Where(type => type.IsPublic && Array.Exists(interesting, name => type.Name.Contains(name, StringComparison.Ordinal)))
                     .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            if (methods.Count > 0)
            {
                TestContext.Out.WriteLine($"### {type.FullName}");
                TestContext.Out.WriteLine(string.Join(", ", methods));
            }
        }
    }

    [Test]
    public void Dump_record_markers()
    {
        var state = BaroquenMelodyArchitecture.Architecture.Types.First(type => string.Equals(type.Name, "CompositionConfigurationState", StringComparison.Ordinal));

        foreach (var member in state.Members)
        {
            TestContext.Out.WriteLine($"{member.GetType().Name}: {member.Name}");
        }

        var nonRecord = BaroquenMelodyArchitecture.Architecture.Types.First(type => string.Equals(type.Name, "MidiGenerator", StringComparison.Ordinal));

        TestContext.Out.WriteLine($"--- MidiGenerator members: {string.Join(", ", nonRecord.Members.Select(member => member.Name))}");
    }

    [Test]
    public void Dump_slice_names_for_single_and_double_star()
    {
        foreach (var pattern in new[] { "BaroquenMelody.Library.(*)", "BaroquenMelody.Library.(**)" })
        {
            var slices = SliceRuleDefinition.Slices().Matching(pattern).GetObjects(BaroquenMelodyArchitecture.Architecture).ToList();

            TestContext.Out.WriteLine($"pattern {pattern}: {slices.Count} slices");

            foreach (var slice in slices.OrderBy(slice => slice.Description, StringComparer.Ordinal))
            {
                TestContext.Out.WriteLine($"  {slice.Description}");
            }
        }
    }

    [Test]
    public void Probe_empty_match_behavior()
    {
        var strict = Types().That().ResideInNamespace("BaroquenMelody.DoesNotExist").Should().BeInternal();
        var relaxed = Types().That().ResideInNamespace("BaroquenMelody.DoesNotExist").Should().BeInternal().WithoutRequiringPositiveResults();

        TestContext.Out.WriteLine($"strict HasNoViolations: {strict.HasNoViolations(BaroquenMelodyArchitecture.Architecture)}");
        TestContext.Out.WriteLine($"relaxed HasNoViolations: {relaxed.HasNoViolations(BaroquenMelodyArchitecture.Architecture)}");
    }
}
