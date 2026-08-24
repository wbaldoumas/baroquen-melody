using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composers;

/// <summary>
///     Seeded end-to-end checks on the shape of the fugue's close: the crafted whole-note final chord, its
///     trailing rest, the meter alignment of the ending, and the raised leading tone under the closing cadence.
/// </summary>
[TestFixture]
internal sealed class EndingCompositionTests
{
    private const int SeedCount = 4;

    // Audit: composers-theme-ending-1
    [Test]
    public void Compose_TheFinalChordSoundsAWholeNoteFollowedByARest()
    {
        var configuration = TestCompositionConfigurations.Get(3, 8) with { ShuffleOrnamentationProcessors = false };

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

    // Audit: composers-theme-ending-2
    [Test]
    public void Compose_TheEndingKeepsEveryMeasureMeterAlignedAndClosesOnADownbeat()
    {
        var configuration = TestCompositionConfigurations.Get(3, 8) with { ShuffleOrnamentationProcessors = false };

        foreach (var seed in Enumerable.Range(1, SeedCount))
        {
            var composition = ComposerGraph.Create(configuration, seed).Composer.Compose(CancellationToken.None);

            composition.Measures.SkipLast(1).Should().OnlyContain(
                measure => measure.Beats.Count == configuration.BeatsPerMeasure,
                $"no measure before the close may be short or over-long, or the barlines drift (seed {seed})");

            composition.Measures[^1].Beats.Should().HaveCount(
                2,
                $"the closing measure is the downbeat whole-note tonic plus its rest (seed {seed})");
        }
    }

    // Audit: composers-theme-ending-3
    [Test]
    public void Compose_InAeolian_TheClosingDominantCarriesTheRaisedLeadingTone()
    {
        var configuration = TestCompositionConfigurations.Get(3, 8, NoteName.A, Mode.Aeolian) with
        {
            ShuffleOrnamentationProcessors = false,
            TonicizationConfiguration = new TonicizationConfiguration(Enabled: true, Probability: 100),
        };

        var closingDominants = new List<(int Seed, BaroquenChord Chord)>();

        foreach (var seed in Enumerable.Range(1, SeedCount))
        {
            var composition = ComposerGraph.Create(configuration, seed).Composer.Compose(CancellationToken.None);
            var penultimateChord = FindPenultimateSoundingChord(composition);

            // The minor v of A Aeolian is E-G-B; a tonicized one is E-G#-B. Either spelling is a closing dominant.
            var isDominant = penultimateChord.Notes.TrueForAll(note => note.NoteName is NoteName.E or NoteName.G or NoteName.GSharp or NoteName.B)
                && penultimateChord.Notes.Exists(note => note.NoteName == NoteName.E)
                && penultimateChord.Notes.Exists(note => note.NoteName is NoteName.G or NoteName.GSharp);

            if (isDominant)
            {
                closingDominants.Add((seed, penultimateChord));
            }
        }

        closingDominants.Should().NotBeEmpty("at least one seed in the sweep closes v -> i");

        foreach (var (seed, chord) in closingDominants)
        {
            chord.Notes.Where(note => note.NoteName is NoteName.G or NoteName.GSharp).Should().OnlyContain(
                note => note.NoteName == NoteName.GSharp,
                $"the closing dominant must be tonicized with the raised leading tone (seed {seed})");
        }
    }

    private static BaroquenChord FindPenultimateSoundingChord(Composition composition)
    {
        var lastMeasure = composition.Measures[^1];

        // The final measure ends [final chord, rest]; the chord before the final chord may sit in the previous measure.
        return lastMeasure.Beats.Count >= 3
            ? lastMeasure.Beats[^3].Chord
            : composition.Measures[^2].Beats[^1].Chord;
    }
}
