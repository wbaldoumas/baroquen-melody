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
///     Covers the composed ending against the production default configuration: every composition must end on a
///     tonic chord (before its closing rest), and across a sweep of seeds the idiomatic cadential trill must
///     sound both at final authentic cadences and at interior phrase seams. Trill coverage sweeps seeds rather
///     than pinning per-seed outcomes because the seeded walks are not identical across operating systems -
///     which seeds happen to cadence authentically differs between local Windows runs and the Linux CI runners.
/// </summary>
[TestFixture]
[Category("Composition")]
[Parallelizable(ParallelScope.All)]
internal sealed class CadentialEndingTests
{
    // MIDI geometry under the generator's 96-ticks-per-quarter time division.
    private const int TicksPerSixteenthNote = 24;

    private const int TicksPerHalfNote = 192;

    [TestCase(3, 1)]
    [TestCase(3, 2)]
    [TestCase(3, 3)]
    [TestCase(3, 42)]
    [TestCase(4, 2)]
    [TestCase(4, 3)]
    [TestCase(4, 8)]
    [TestCase(4, 42)]
    public void Compose_WithProductionDefaults_EndsOnTheTonic(int voices, int seed)
    {
        // arrange
        var configuration = BuildProductionConfiguration(voices);

        // act
        var composition = SeededComposition.Compose(configuration, seed);

        // assert - the composition ends on a tonic chord
        var notes = composition.MidiFile.GetNotes().ToList();
        var finalOnset = notes.Max(static note => note.Time);
        var finalChordPitchClasses = notes.Where(note => note.Time == finalOnset).Select(static note => (int)(note.NoteNumber % 12));

        finalChordPitchClasses.Should().OnlyContain(
            static pitchClass => pitchClass == 0 || pitchClass == 4 || pitchClass == 7,
            "the composition should end on a C major tonic chord");
    }

    [TestCase(3)]
    [TestCase(4)]
    public void Compose_WithProductionDefaults_SomeSeedEndsWithTheCadentialTrill(int voices)
    {
        // arrange
        var configuration = BuildProductionConfiguration(voices);

        // act & assert - endings that achieve an authentic cadence carry the trill on the dominant's leading
        // tone; the sweep stops at the first seed that renders one
        Enumerable.Range(1, 12)
            .Any(seed =>
            {
                var notes = SeededComposition.Compose(configuration, seed).MidiFile.GetNotes().ToList();
                var finalOnset = notes.Max(static note => note.Time);

                return FindCadentialTrillOnsets(notes).Exists(onset => onset >= finalOnset - TicksPerHalfNote);
            })
            .Should().BeTrue("some seeded ending must achieve an authentic cadence and carry the cadential trill");
    }

    [Test]
    public void Compose_WithProductionDefaults_SomeSeedAppliesTheCadentialTrillAtAnInteriorPhraseSeam()
    {
        // arrange - three voices keeps the sweep cheap; the seam-trill mechanism is voice-count independent
        var configuration = BuildProductionConfiguration(voices: 3);

        // act & assert - phrase seams that classify as authentic cadences carry the cadential trill
        // mid-composition, not just at the final cadence; the sweep stops at the first seed that renders one
        Enumerable.Range(1, 25)
            .Any(seed =>
            {
                var notes = SeededComposition.Compose(configuration, seed).MidiFile.GetNotes().ToList();
                var finalOnset = notes.Max(static note => note.Time);

                return FindCadentialTrillOnsets(notes).Exists(onset => onset < finalOnset - TicksPerHalfNote);
            })
            .Should().BeTrue("some seeded phrase seam must classify as an authentic cadence and carry the cadential trill");
    }

    /// <summary>
    ///     Finds the onset of every cadential trill in the rendered notes: a voice playing the eight-sixteenth
    ///     leading-tone alternation (B C B C B C A B) in contiguous time.
    /// </summary>
    private static List<long> FindCadentialTrillOnsets(List<Melanchall.DryWetMidi.Interaction.Note> notes)
    {
        var trillPitchClasses = new[] { 11, 0, 11, 0, 11, 0, 9, 11 };

        return notes
            .Where(static note => note.Length == TicksPerSixteenthNote)
            .GroupBy(static note => note.Channel)
            .SelectMany(channelNotes =>
            {
                var orderedNotes = channelNotes.OrderBy(static note => note.Time).ToList();
                var trillOnsets = new List<long>();

                for (var startIndex = 0; startIndex <= orderedNotes.Count - trillPitchClasses.Length; startIndex++)
                {
                    var isTrill = true;

                    for (var offset = 0; offset < trillPitchClasses.Length; offset++)
                    {
                        if (orderedNotes[startIndex + offset].NoteNumber % 12 != trillPitchClasses[offset] ||
                            (offset > 0 && orderedNotes[startIndex + offset].Time != orderedNotes[startIndex + offset - 1].Time + TicksPerSixteenthNote))
                        {
                            isTrill = false;
                            break;
                        }
                    }

                    if (isTrill)
                    {
                        trillOnsets.Add(orderedNotes[startIndex].Time);
                    }
                }

                return trillOnsets;
            })
            .ToList();
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
