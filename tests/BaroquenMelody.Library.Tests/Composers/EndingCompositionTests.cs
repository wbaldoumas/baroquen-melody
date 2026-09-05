using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Enums.Extensions;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composers;

/// <summary>
///     Seeded end-to-end checks on the shape of the fugue's close: the ending composer's crafted whole-note
///     final chord and its trailing rest must survive every later pass, whatever the meter's default grid.
/// </summary>
[TestFixture]
[Category("Composition")]
[Parallelizable(ParallelScope.All)]
internal sealed class EndingCompositionTests
{
    private const int SeedCount = 4;

    [TestCase(Meter.FourFour)]
    [TestCase(Meter.ThreeFour)]
    public void Compose_TheFinalChordSoundsAWholeNoteFollowedByARest(Meter meter)
    {
        var configuration = TestCompositionConfigurations.Get(3, 8) with
        {
            Meter = meter,
            DefaultNoteTimeSpan = meter.DefaultMusicalTimeSpan(),
            ShuffleOrnamentationProcessors = false,
        };

        foreach (var seed in Enumerable.Range(1, SeedCount))
        {
            var composition = ComposerGraph.Create(configuration, seed).Composer.Compose(CancellationToken.None);
            var lastMeasure = composition.Measures[^1];

            lastMeasure.Beats[^2].Chord.Notes.Should().OnlyContain(
                note => note.MusicalTimeSpan == MusicalTimeSpan.Whole && note.OrnamentationType == OrnamentationType.None,
                $"the ending composer's whole-note final chord must survive the sustain pass (seed {seed})");

            lastMeasure.Beats[^1].Chord.Notes.Should().OnlyContain(
                note => note.OrnamentationType == OrnamentationType.Rest,
                $"the trailing beat after the final chord is a rest in every voice (seed {seed})");
        }
    }
}
