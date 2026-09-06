using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Motifs.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using CsCheck;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Motifs;

/// <summary>
///     End-to-end coverage for motivic development: it must keep the composition deterministic under a seed and every
///     musical invariant intact, while actually changing the music relative to verbatim repetition.
/// </summary>
[TestFixture]
[Category(TestCategories.Composition)]
[Parallelizable(ParallelScope.All)]
internal sealed class MotifDevelopmentCompositionTests
{
    private const int SampleIterations = 5;

    private static readonly MotifDevelopmentConfiguration AlwaysDevelop = new(true, [MotifTransform.Invert, MotifTransform.Retrograde], 100, MotifDevelopmentScope.SingleVoice);

    private static readonly MotifDevelopmentConfiguration NeverDevelop = new(false, [MotifTransform.Invert, MotifTransform.Retrograde], 100, MotifDevelopmentScope.SingleVoice);

    [Test]
    public void Compose_WithDevelopmentEnabled_IsDeterministicUnderASeed() => Gen.Int.Sample(
        seed =>
        {
            var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false, MotifDevelopmentConfiguration = AlwaysDevelop };

            var first = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));
            var second = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

            first.Should().NotBeEmpty();
            first.Should().Equal(second, "transform and voice selection must flow through the seeded random provider");
        },
        iter: SampleIterations
    );

    [Test]
    public void ComposedNotes_WithDevelopmentEnabled_SatisfyMusicalInvariants()
    {
        // The default configuration composes in C Ionian, where the tonicization pass may raise the
        // thirds of the minor triads (F# from ii, G# from iii, C# from vi) anywhere in the piece,
        // developed phrases included, so those licensed alterations are as invariant-clean as scale tones.
        var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false, MotifDevelopmentConfiguration = AlwaysDevelop };
        var scaleNoteNumbers = configuration.Scale.GetNotes().Select(static note => (int)note.NoteNumber).ToHashSet();
        var licensedPitchClasses = new HashSet<NoteName> { NoteName.FSharp, NoteName.GSharp, NoteName.CSharp };
        var minVelocity = configuration.InstrumentConfigurations.Min(static instrument => (int)instrument.MinVelocity);
        var maxVelocity = configuration.InstrumentConfigurations.Max(static instrument => (int)instrument.MaxVelocity);

        Gen.Int.Sample(
            seed =>
            {
                var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

                notes.Should().NotBeEmpty("every composition must produce notes");
                notes.Should().OnlyContain(
                    note => scaleNoteNumbers.Contains(note.NoteNumber) || licensedPitchClasses.Contains((NoteName)(note.NoteNumber % 12)),
                    "every developed pitch must stay diatonic or carry a tonicization license");
                notes.Should().OnlyContain(note => note.Velocity >= minVelocity && note.Velocity <= maxVelocity, "velocity must stay within the configured dynamic range");
                notes.Should().OnlyContain(note => note.NoteNumber >= 0 && note.NoteNumber <= 127, "every note number must be a valid MIDI pitch");
            },
            iter: SampleIterations
        );
    }

    [Test]
    public void ComposedNotes_WithDevelopmentEnabledInAModeWithoutALiftedGate_StayFullyDiatonic()
    {
        // The default-configuration invariant above had to admit licensed tonicization alterations, so
        // this case keeps the strict form alive: in Dorian (a mode whose tonicization gate has not
        // lifted) no chromatic license exists, and developed music must stay entirely diatonic - a
        // permanent net against chromatic leakage from any code path.
        var configuration = TestCompositionConfigurations.Get(3, 10, tonic: NoteName.D, mode: Mode.Dorian) with { ShuffleOrnamentationProcessors = false, MotifDevelopmentConfiguration = AlwaysDevelop };
        var scaleNoteNumbers = configuration.Scale.GetNotes().Select(static note => (int)note.NoteNumber).ToHashSet();

        Gen.Int.Sample(
            seed =>
            {
                var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

                notes.Should().OnlyContain(note => scaleNoteNumbers.Contains(note.NoteNumber), "every developed pitch must stay diatonic");
            },
            iter: SampleIterations
        );
    }

    [Test]
    public void Compose_WithDevelopmentEnabledVersusDisabled_ProducesDifferentMusic()
    {
        // arrange
        var enabled = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false, MotifDevelopmentConfiguration = AlwaysDevelop };
        var disabled = enabled with { MotifDevelopmentConfiguration = NeverDevelop };

        // act: a coarse end-to-end smoke check that enabling development changes the rendered composition for at least one
        // seed (the per-transform pitch correctness is pinned by the MotifDeveloper unit tests). Some developed candidates
        // fall back to verbatim when the rule gate rejects them, so we look across several seeds.
        var anySeedDiffers = Enumerable.Range(1, 25).Any(seed =>
        {
            var developed = SeededComposition.Notes(SeededComposition.Compose(enabled, seed));
            var verbatim = SeededComposition.Notes(SeededComposition.Compose(disabled, seed));

            return !developed.SequenceEqual(verbatim);
        });

        // assert
        anySeedDiffers.Should().BeTrue("enabling motivic development must change the rendered composition end-to-end");
    }
}
