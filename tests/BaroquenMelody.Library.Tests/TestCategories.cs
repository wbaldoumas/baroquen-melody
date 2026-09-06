namespace BaroquenMelody.Library.Tests;

/// <summary>
///     The NUnit categories Stryker's <c>test-case-filter</c> (in <c>stryker-config.json</c>) excludes from the
///     Library's mutation runs. A test carrying one of these never runs against a mutant, so every declaration here
///     is a deliberate trade of kills for run time, and <c>MutationUniverseTests</c> in the Architecture suite holds
///     the filter to exactly this list and the suite to exactly these names: by design, this suite's only categories
///     are Stryker exclusions. Apply them through attributes only (<c>[Category]</c>, or the <c>Category</c> property
///     of <c>[TestFixture]</c>, <c>[TestCase]</c> and <c>[TestCaseSource]</c>): the guards read attributes, and a
///     category set at run time (<c>TestCaseData.SetCategory</c>, a <c>Property</c> attribute named Category) would
///     be invisible to them.
/// </summary>
internal static class TestCategories
{
    /// <summary>
    ///     The seeded composition sweeps: fixtures that compose many pieces to assert an existence property. They are
    ///     most of the suite's wall time, so CI runs them in their own shards (<c>composition-shards.json</c>; the
    ///     fixture is tagged, never a method, and goes into one shard) and neither the unit leg nor Stryker runs them.
    /// </summary>
    public const string Composition = "Composition";

    /// <summary>
    ///     A test that composes a whole piece, or several, to pin one property of the pipeline as a whole: that a
    ///     seed reproduces, or that a mode and meter configure and compose. CI's unit leg runs it like any other test;
    ///     Stryker does not. Such a test covers nearly every mutant in the Library, so it ran against nearly every one
    ///     of them, and at seconds per composition the tagged tests were about 70 % of a mutant's test time while
    ///     catching almost nothing the fast unit tests miss (5 of 1,657 kills when measured). Tag the test, not the
    ///     fixture, unless every test in the fixture composes; keep one fast representative untagged wherever a
    ///     whole-piece smoke test is the only coverage a path has.
    /// </summary>
    public const string WholeComposition = "WholeComposition";
}
