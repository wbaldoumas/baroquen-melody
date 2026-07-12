using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests;

/// <summary>
///     Pins the fugal exposition against the PRODUCTION default instrument ranges: the composition must open with a
///     single voice stating the theme, not the all-voices fallback. The CI test ranges are geometrically friendlier
///     (Instrument.One's floor is C4, not C5), which masked the 4-voice interaction between fugal entry placement
///     and the default-enabled voice spacing rule.
/// </summary>
[TestFixture]
internal sealed class FugalExpositionTests
{
    [TestCase(3, 1)]
    [TestCase(3, 42)]
    [TestCase(4, 1)]
    [TestCase(4, 42)]
    public void Compose_WithProductionDefaultRanges_OpensWithASingleFugalEntry(int voices, int seed)
    {
        // arrange
        var configuration = BuildProductionConfiguration(voices);

        // act
        var composition = SeededComposition.Compose(configuration, seed);

        // assert: a fugal exposition opens with exactly one voice sounding at tick zero
        var voicesAtTickZero = composition.MidiFile.GetNotes().Count(static note => note.Time == 0);

        voicesAtTickZero.Should().Be(1, "the composition should open with a single staggered fugal entry, not the all-voices fallback theme");
    }

    private static CompositionConfiguration BuildProductionConfiguration(int voices)
    {
        var instrumentConfigurations = InstrumentConfiguration.DefaultConfigurations.Values
            .OrderBy(static instrumentConfiguration => instrumentConfiguration.Instrument)
            .Take(voices)
            .Select(static instrumentConfiguration => instrumentConfiguration with { Status = ConfigurationStatus.Enabled })
            .ToHashSet();

        return new CompositionConfiguration(
            instrumentConfigurations,
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 3
        ) with
        { ShuffleOrnamentationProcessors = false };
    }
}
