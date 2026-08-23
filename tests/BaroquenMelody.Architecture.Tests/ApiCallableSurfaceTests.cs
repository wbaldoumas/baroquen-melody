using ArchUnitNET.Fluent;
using NUnit.Framework;
using System.Reflection;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Research probes: prints every public method callable on the live fluent objects (predicates and
///     conditions live on generic base classes, so a type-name scan misses them) and the Freeze API shape.
/// </summary>
[TestFixture]
internal sealed class ApiCallableSurfaceTests
{
    [Test]
    public void Dump_callable_surface()
    {
        Dump("Types()", Types());
        Dump("Types().That()", Types().That());
        Dump("Types().That().ResideInAssembly(lib)", Types().That().ResideInAssembly(BaroquenMelodyArchitecture.Library));
        Dump("Types()...Should()", Types().That().ResideInAssembly(BaroquenMelodyArchitecture.Library).Should());
        Dump("Classes().That()", Classes().That());
        Dump("Classes()...Should()", Classes().That().ResideInAssembly(BaroquenMelodyArchitecture.Library).Should());
        Dump("Interfaces().That()", Interfaces().That());
        Dump("MethodMembers().That()", MethodMembers().That());
        Dump("Members().That()", Members().That());
    }

    [Test]
    public void Dump_freeze_api()
    {
        foreach (var type in typeof(ArchRuleDefinition).Assembly.GetTypes()
                     .Where(type => type.Namespace?.StartsWith("ArchUnitNET.Fluent.Freeze", StringComparison.Ordinal) == true)
                     .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            TestContext.Out.WriteLine($"### {type.FullName} (public={type.IsPublic})");

            foreach (var constructor in type.GetConstructors())
            {
                TestContext.Out.WriteLine($"  ctor({string.Join(", ", constructor.GetParameters().Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"))})");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                TestContext.Out.WriteLine($"  {(method.IsStatic ? "static " : string.Empty)}{method.Name}({string.Join(", ", method.GetParameters().Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"))})");
            }
        }
    }

    [Test]
    public void Dump_interface_and_member_name_formats()
    {
        var architecture = BaroquenMelodyArchitecture.Architecture;

        TestContext.Out.WriteLine($"total interfaces: {architecture.Interfaces.Count()}");

        foreach (var candidate in architecture.Interfaces.Where(candidate => candidate.Name.Contains("CompositionRule", StringComparison.Ordinal)))
        {
            TestContext.Out.WriteLine($"interface: '{candidate.FullName}' (name '{candidate.Name}')");
        }

        foreach (var member in architecture.Types
                     .SelectMany(type => type.Members)
                     .Where(member => member.Name.Contains("ObserveChanges", StringComparison.Ordinal))
                     .Take(10))
        {
            TestContext.Out.WriteLine($"member: '{member.FullName}' (name '{member.Name}') declared in {member.DeclaringType.FullName}");
        }
    }

    private static void Dump(string label, object fluentObject)
    {
        var methods = fluentObject.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        TestContext.Out.WriteLine($"### {label} => {fluentObject.GetType().Name}");
        TestContext.Out.WriteLine(string.Join(", ", methods));
    }
}
