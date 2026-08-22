using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class ArpeggioProcessorTests
{
    private static readonly ChordNumber[] AllChordNumbers =
    [
        ChordNumber.I,
        ChordNumber.II,
        ChordNumber.III,
        ChordNumber.IV,
        ChordNumber.V,
        ChordNumber.VI,
        ChordNumber.VII
    ];

    [TestCase(Mode.Ionian)]
    [TestCase(Mode.Aeolian)]
    public void Process_sounds_only_chord_tones_for_every_diatonic_triad(Mode mode)
    {
        // arrange - the arpeggio's design claim is that its fixed scale-step offsets land exact chord
        // tones under the matching degree gate for EVERY diatonic triad, whatever the triad's quality;
        // this decides that claim over all seven chords of a major and a minor scale
        var compositionConfiguration = TestCompositionConfigurations.Get(mode: mode);

        var configurations = new OrnamentationProcessorConfigurationFactory(
            new ChordNumberIdentifier(compositionConfiguration),
            new WeightedRandomBooleanGenerator(),
            compositionConfiguration,
            Substitute.For<ILogger>()
        ).Create(
            new OrnamentationConfiguration(OrnamentationType.Arpeggio, ConfigurationStatus.Enabled, Probability: 100)
        ).ToList();

        var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

        var rootProcessor = new OrnamentationProcessor(
            musicalTimeSpanCalculator,
            compositionConfiguration,
            configurations.Single(static configuration => configuration.InputPolicies.Any(static policy => policy is IsRootOfChord))
        );

        var thirdProcessor = new OrnamentationProcessor(
            musicalTimeSpanCalculator,
            compositionConfiguration,
            configurations.Single(static configuration => configuration.InputPolicies.Any(static policy => policy is IsThirdOfChord))
        );

        var fifthProcessor = new OrnamentationProcessor(
            musicalTimeSpanCalculator,
            compositionConfiguration,
            configurations.Single(static configuration => configuration.InputPolicies.Any(static policy => policy is IsFifthOfChord))
        );

        foreach (var chordNumber in AllChordNumbers)
        {
            var chordTriad = ChordTriad.FromChordNumber(compositionConfiguration.Scale, chordNumber);

            chordTriad.Should().NotBeNull("every non-Unknown chord number resolves a triad");

            var triad = chordTriad!.Value;

            // act + assert - from each degree the cell must traverse only the sounding triad's own tones
            AssertCell(rootProcessor, triad.Root, [triad.Fifth, triad.Third, triad.Fifth], chordNumber);
            AssertCell(thirdProcessor, triad.Third, [triad.Root, triad.Fifth, triad.Root], chordNumber);
            AssertCell(fifthProcessor, triad.Fifth, [triad.Third, triad.Root, triad.Third], chordNumber);
        }
    }

    private static void AssertCell(
        OrnamentationProcessor processor,
        NoteName principalNoteName,
        NoteName[] expectedCellNoteNames,
        ChordNumber chordNumber
    )
    {
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Note.Get(principalNoteName, 4), MusicalTimeSpan.Half)])),
            NextBeat: null
        );

        processor.Process(ornamentationItem);

        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Arpeggio);

        noteToAssert.Ornamentations
            .Select(static ornamentation => ornamentation.Raw.NoteName)
            .Should()
            .Equal(
                expectedCellNoteNames,
                "the {0} cell from {1} must sound only the sounding chord's own tones",
                chordNumber,
                principalNoteName
            );
    }
}
