using NUnit.Framework;
using System.Reflection;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Reads the NUnit categories a fixture or a test carries, the way the NUnit adapter's <c>TestCategory</c>
///     filter sees them: <c>[Category]</c> attributes, plus the comma-separated <c>Category</c> property of
///     <c>[TestFixture]</c>, <c>[TestCase]</c> and <c>[TestCaseSource]</c>. A guard that scanned <c>[Category]</c>
///     alone would let a fixture tagged through <c>[TestFixture(Category = "Composition")]</c> slip past.
/// </summary>
internal static class TestCategoryReflection
{
    public static IEnumerable<string> OfFixture(Type type) => type
        .GetCustomAttributes<CategoryAttribute>(inherit: true)
        .Select(static category => category.Name)
        .Concat(type.GetCustomAttributes<TestFixtureAttribute>(inherit: true).SelectMany(static fixture => Split(fixture.Category)))
        .Distinct(StringComparer.Ordinal);

    public static IEnumerable<string> OfTest(MethodInfo method) => method
        .GetCustomAttributes<CategoryAttribute>(inherit: true)
        .Select(static category => category.Name)
        .Concat(method.GetCustomAttributes<TestCaseAttribute>(inherit: true).SelectMany(static testCase => Split(testCase.Category)))
        .Concat(method.GetCustomAttributes<TestCaseSourceAttribute>(inherit: true).SelectMany(static source => Split(source.Category)))
        .Distinct(StringComparer.Ordinal);

    public static IEnumerable<MethodInfo> TestsOf(Type type) => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

    private static string[] Split(string? categories) => (categories ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
