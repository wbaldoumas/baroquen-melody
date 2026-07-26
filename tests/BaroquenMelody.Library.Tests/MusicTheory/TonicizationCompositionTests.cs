using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.MusicTheory;

/// <summary>
///     End-to-end coverage for tonicization. The applicator draws randomness only after the walk is fully
///     composed and judges eligibility on diatonic chord numbers, so enabling it can never change which
///     chords are composed or how long the piece is - only which pitches are raised. Ornamentation is a
///     participant in every raise: figures in all voices are respelled into the tonicized harmony, never
///     stripped. The mode gate is lifted for Ionian and Aeolian; in the still-gated modes the pass
///     consumes nothing, so output is byte-identical either way. Trill-style seed sweeps are used instead
///     of per-seed pins because seeded walks differ across operating systems.
/// </summary>
[TestFixture]
internal sealed class TonicizationCompositionTests
{
    private const int SeedCount = 5;

    // One beat of the default half-note time span at DryWetMidi's default 96 ticks per quarter note.
    private const long TicksPerBeat = 192;

    // A Aeolian and C Ionian are relative keys, so their licensed alterations are the same three pitch
    // classes: in Aeolian, G# from v, C# from i, and F# from iv; in Ionian, F# from ii, G# from iii,
    // and C# from vi. In both, F# doubles as the courtesy neighbor below a raised G#.
    private static readonly HashSet<NoteName> LicensedPitchClasses = [NoteName.GSharp, NoteName.CSharp, NoteName.FSharp];

    [Test]
    public void Compose_InAModeWithoutALiftedGate_IsByteIdenticalWhetherTonicizationIsEnabledOrDisabled()
    {
        // arrange - the license gate lifts mode by mode: in Dorian the pass returns before consuming
        // any randomness, so the output must be identical note for note
        var enabled = TestCompositionConfigurations.Get(3, 10, tonic: NoteName.D, mode: Mode.Dorian) with { ShuffleOrnamentationProcessors = false };
        var disabled = enabled with { TonicizationConfiguration = new TonicizationConfiguration(Enabled: false, Probability: 0) };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var tonicized = SeededComposition.Notes(SeededComposition.Compose(enabled, seed));
            var plain = SeededComposition.Notes(SeededComposition.Compose(disabled, seed));

            // assert
            tonicized.Should().Equal(plain, "tonicization must be inert in a still-gated mode for seed {0}", seed);
        }
    }

    [TestCase(NoteName.A, Mode.Aeolian)]
    [TestCase(NoteName.C, Mode.Ionian)]
    public void Compose_InALiftedMode_NeverChangesTheDuration(NoteName tonic, Mode mode)
    {
        // arrange
        var enabled = TestCompositionConfigurations.Get(3, 10, tonic: tonic, mode: mode) with { ShuffleOrnamentationProcessors = false };
        var disabled = enabled with { TonicizationConfiguration = new TonicizationConfiguration(Enabled: false, Probability: 0) };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var tonicized = SeededComposition.Notes(SeededComposition.Compose(enabled, seed));
            var plain = SeededComposition.Notes(SeededComposition.Compose(disabled, seed));

            // assert - with the pass fully disabled the control consumes no tonicization draws, so the
            // two runs' downstream draws diverge and the sustain pass may merge different repeats; but
            // no pass ever changes a beat's total time, so even under that divergence the composition's
            // duration is exact - toggling the feature can never change how long a piece is
            var tonicizedFinalTick = tonicized.Max(static note => note.Time + note.Length);
            var plainFinalTick = plain.Max(static note => note.Time + note.Length);

            tonicizedFinalTick.Should().Be(plainFinalTick, "tonicization must not change the composition's duration for seed {0}", seed);
        }
    }

    [TestCase(NoteName.A, Mode.Aeolian)]
    [TestCase(NoteName.C, Mode.Ionian)]
    public void Compose_InALiftedMode_NeverStripsAFigure(NoteName tonic, Mode mode)
    {
        // arrange - the control must consume the same randomness as the full-probability run, so it
        // keeps the pass enabled at probability zero: eligibility scanning is deterministic and each
        // eligible site draws exactly once regardless of outcome, which keeps every downstream draw
        // (the sustain pass draws per candidate) aligned between the two runs. The only differences
        // left are the raises themselves - and since figures are raised, never reset, the raised run
        // can only gain note events (a raise can keep two repeated notes from merging in the sustain
        // pass), never lose them
        var raised = TestCompositionConfigurations.Get(3, 10, tonic: tonic, mode: mode) with { ShuffleOrnamentationProcessors = false };
        var control = raised with { TonicizationConfiguration = new TonicizationConfiguration(Enabled: true, Probability: 0) };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var tonicized = SeededComposition.Notes(SeededComposition.Compose(raised, seed));
            var plain = SeededComposition.Notes(SeededComposition.Compose(control, seed));

            // assert
            tonicized.Count.Should().BeGreaterThanOrEqualTo(plain.Count, "tonicization must never strip a figure for seed {0}", seed);
        }
    }

    [TestCase(NoteName.A, Mode.Aeolian)]
    [TestCase(NoteName.C, Mode.Ionian)]
    public void Compose_InALiftedMode_NeverSoundsANaturalAgainstItsRaisedForm(NoteName tonic, Mode mode)
    {
        // arrange - every figure is respelled into the tonicized harmony, so within any beat no raised
        // pitch class may sound against its own natural: no G beside G#, no C beside C#, and F beside F#
        // only in a beat that also sounds G#, where the F# is the courtesy below the raised third and an
        // F natural elsewhere in the texture is idiomatic modal color. The same three rules govern both
        // relative keys because they license the same pitch classes
        var configuration = TestCompositionConfigurations.Get(3, 10, tonic: tonic, mode: mode) with { ShuffleOrnamentationProcessors = false };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

            var pitchClassesByBeat = new Dictionary<long, HashSet<NoteName>>();

            foreach (var note in notes)
            {
                var firstBeat = note.Time / TicksPerBeat;
                var lastBeat = (note.Time + note.Length - 1) / TicksPerBeat;

                for (var beat = firstBeat; beat <= lastBeat; beat++)
                {
                    if (!pitchClassesByBeat.TryGetValue(beat, out var pitchClasses))
                    {
                        pitchClasses = [];
                        pitchClassesByBeat[beat] = pitchClasses;
                    }

                    pitchClasses.Add((NoteName)(note.NoteNumber % 12));
                }
            }

            // assert
            foreach (var (beat, pitchClasses) in pitchClassesByBeat)
            {
                (pitchClasses.Contains(NoteName.G) && pitchClasses.Contains(NoteName.GSharp))
                    .Should().BeFalse("beat {0} of seed {1} must not sound G natural against G#", beat, seed);
                (pitchClasses.Contains(NoteName.C) && pitchClasses.Contains(NoteName.CSharp))
                    .Should().BeFalse("beat {0} of seed {1} must not sound C natural against C#", beat, seed);
                (pitchClasses.Contains(NoteName.F) && pitchClasses.Contains(NoteName.FSharp) && !pitchClasses.Contains(NoteName.GSharp))
                    .Should().BeFalse("beat {0} of seed {1} may only mix F and F# under a raised G#", beat, seed);
            }
        }
    }

    [TestCase(NoteName.A, Mode.Aeolian)]
    [TestCase(NoteName.C, Mode.Ionian)]
    public void Compose_InALiftedMode_RendersARaisedThirdForSomeSeed(NoteName tonic, Mode mode)
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 10, tonic: tonic, mode: mode) with { ShuffleOrnamentationProcessors = false };

        // act & assert - some seeded composition must sound a licensed chromatic pitch: a real dominant
        // where the diatonic walk had none
        Enumerable.Range(1, 12)
            .Any(seed => SeededComposition.Notes(SeededComposition.Compose(configuration, seed))
                .Any(static note => LicensedPitchClasses.Contains((NoteName)(note.NoteNumber % 12))))
            .Should().BeTrue("some seeded composition must render a raised third");
    }
}
