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
///     Pins the composed ending against the production default configuration: every composition must end on a
///     tonic chord (before its closing rest), and seeds whose ending achieves an authentic cadence must carry
///     the idiomatic cadential trill on the leading tone of the dominant.
/// </summary>
[TestFixture]
internal sealed class CadentialEndingTests
{
    // MIDI geometry under the generator's 96-ticks-per-quarter time division.
    private const int TicksPerSixteenthNote = 24;

    private const int TicksPerHalfNote = 192;

    [TestCase(3, 1, true)]
    [TestCase(3, 2, false)]
    [TestCase(3, 3, true)]
    [TestCase(3, 42, false)]
    [TestCase(4, 2, true)]
    [TestCase(4, 3, false)]
    [TestCase(4, 8, true)]
    [TestCase(4, 42, false)]
    public void Compose_WithProductionDefaults_EndsWithATonicCadence(int voices, int seed, bool expectCadentialTrill)
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

        if (!expectCadentialTrill)
        {
            return;
        }

        // assert - the beat before the final chord carries the cadential trill in one voice: the leading tone
        // alternating with its upper neighbor, closed by the lower-neighbor termination (B C B C B C A B)
        var trillPitchClasses = new[] { 11, 0, 11, 0, 11, 0, 9, 11 };

        var voiceHasCadentialTrill = notes
            .Where(note => note.Time >= finalOnset - TicksPerHalfNote && note.Time < finalOnset)
            .Where(static note => note.Length == TicksPerSixteenthNote)
            .GroupBy(static note => note.Channel)
            .Select(static channelNotes => channelNotes.OrderBy(static note => note.Time).Select(static note => (int)(note.NoteNumber % 12)))
            .Any(pitchClasses => pitchClasses.SequenceEqual(trillPitchClasses));

        voiceHasCadentialTrill.Should().BeTrue("the ending forms an authentic cadence, so the dominant's leading tone should carry the cadential trill");
    }

    [TestCase(3, 3)]
    [TestCase(3, 10)]
    [TestCase(4, 10)]
    public void Compose_WithProductionDefaults_AppliesTheCadentialTrillAtInteriorPhraseSeams(int voices, int seed)
    {
        // arrange
        var configuration = BuildProductionConfiguration(voices);

        // act
        var composition = SeededComposition.Compose(configuration, seed);

        // assert - for these pinned seeds, a phrase seam mid-composition classifies as an authentic cadence, so
        // the cadential trill must sound somewhere before the final cadence window - proving the seam trill is
        // live and not just the ending's
        var notes = composition.MidiFile.GetNotes().ToList();
        var finalOnset = notes.Max(static note => note.Time);

        FindCadentialTrillOnsets(notes).Should().Contain(
            onset => onset < finalOnset - TicksPerHalfNote,
            because: "a phrase seam forming an authentic cadence carries the cadential trill mid-composition");
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
