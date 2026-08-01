using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using CsCheck;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests;

/// <summary>
///     Properties that must hold for every generated composition, regardless of seed. These pin current behavior and
///     become acceptance tests for future rules.
/// </summary>
/// <remarks>
///     Asserted over the generated MIDI, these cover: non-emptiness, diatonic pitch content, velocity within the
///     configured dynamic range, and valid MIDI pitch numbers. Structural / voice-leading invariants (final-chord
///     resolution, parallel perfect intervals, voice crossing) and the strict per-instrument pitch range operate on
///     the in-memory <c>Composition</c> rather than the ornamented MIDI (ornament sub-notes intentionally extend
///     beyond the base instrument range).
///     <para>
///         Voice crossing and overlap are now enforced during body composition by the default-enabled
///         <c>AvoidVoiceCrossing</c> / <c>AvoidVoiceOverlap</c> rules (see their unit tests), but a full-composition
///         "no crossing" invariant is intentionally not asserted here: the flattened MIDI carries no per-voice
///         identity, and while the thematic <c>GenerateInitialChord</c> now validates its candidates against the
///         rules, it degrades to an unvalidated voicing when its bounded retry is exhausted. A MIDI/composition-level
///         crossing invariant is deferred until the composition exposes its in-memory voices.
///     </para>
/// </remarks>
[TestFixture]
internal sealed class MusicalInvariantTests
{
    private const int SampleIterations = 5;

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)] // the default four-voice texture is the hardest case: voice spacing constrains two adjacent pairs
    public void ComposedNotes_SatisfyMusicalInvariants(int numberOfInstruments)
    {
        // The default configuration composes in C Ionian, where the tonicization pass may raise the
        // thirds of the minor triads - F# from ii, G# from iii, C# from vi - so a chromatic pitch
        // carrying one of those licenses is as invariant-clean as a scale tone.
        var configuration = TestCompositionConfigurations.Get(numberOfInstruments, 10) with { ShuffleOrnamentationProcessors = false };
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
                    "every pitch must be diatonic to the configured scale or a licensed tonicization alteration");
                notes.Should().OnlyContain(note => note.Velocity >= minVelocity && note.Velocity <= maxVelocity, "velocity must stay within the configured dynamic range");
                notes.Should().OnlyContain(note => note.NoteNumber >= 0 && note.NoteNumber <= 127, "every note number must be a valid MIDI pitch");
            },
            iter: SampleIterations
        );
    }

    [TestCase(2, NoteName.A, Mode.Aeolian)]
    [TestCase(3, NoteName.A, Mode.Aeolian)]
    [TestCase(2, NoteName.C, Mode.Ionian)]
    [TestCase(3, NoteName.C, Mode.Ionian)]
    public void ComposedNotes_InALiftedMode_ContainOnlyScaleTonesAndLicensedAlterations(int numberOfInstruments, NoteName tonic, Mode mode)
    {
        // The tonicization pass may raise the thirds of the mode's minor triads and nothing else. A
        // Aeolian (G# from v, C# from i, F# from iv) and its relative C Ionian (F# from ii, G# from
        // iii, C# from vi) license the same three pitch classes, with F# doubling as the whole-step
        // courtesy neighbor below a raised G# in both: every chromatic pitch in the output must carry
        // one of those licenses.
        var configuration = TestCompositionConfigurations.Get(numberOfInstruments, 10, tonic: tonic, mode: mode) with { ShuffleOrnamentationProcessors = false };
        var scaleNoteNumbers = configuration.Scale.GetNotes().Select(static note => (int)note.NoteNumber).ToHashSet();
        var licensedPitchClasses = new HashSet<NoteName> { NoteName.GSharp, NoteName.CSharp, NoteName.FSharp };

        Gen.Int.Sample(
            seed =>
            {
                var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

                notes.Should().OnlyContain(
                    note => scaleNoteNumbers.Contains(note.NoteNumber) || licensedPitchClasses.Contains((NoteName)(note.NoteNumber % 12)),
                    "every pitch must be diatonic or a licensed tonicization alteration");
            },
            iter: SampleIterations
        );
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void ComposedNotes_InGroundBassForm_SatisfyMusicalInvariants(int numberOfInstruments)
    {
        // The ground bass form replaces the whole fugal pipeline with its own composer, so every
        // invariant the standard form guarantees must hold here too - including the tonicization
        // licenses, which fire especially often over a ground whose statement seams are all
        // dominant-to-tonic pairs.
        var configuration = TestCompositionConfigurations.Get(numberOfInstruments, 10) with
        {
            ShuffleOrnamentationProcessors = false,
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true)
        };
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
                    "every pitch must be diatonic to the configured scale or a licensed tonicization alteration");
                notes.Should().OnlyContain(note => note.Velocity >= minVelocity && note.Velocity <= maxVelocity, "velocity must stay within the configured dynamic range");
                notes.Should().OnlyContain(note => note.NoteNumber >= 0 && note.NoteNumber <= 127, "every note number must be a valid MIDI pitch");
            },
            iter: SampleIterations
        );
    }

    [TestCase(2)]
    [TestCase(3)]
    public void ComposedNotes_InGroundBassFormInAModeWithoutALiftedGate_StayFullyDiatonic(int numberOfInstruments)
    {
        // The strict all-diatonic net must survive the new form exactly as it survives the standard
        // one: in a still-gated mode no code path may leak a chromatic pitch over the ground.
        var configuration = TestCompositionConfigurations.Get(numberOfInstruments, 10, tonic: NoteName.D, mode: Mode.Dorian) with
        {
            ShuffleOrnamentationProcessors = false,
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true)
        };
        var scaleNoteNumbers = configuration.Scale.GetNotes().Select(static note => (int)note.NoteNumber).ToHashSet();

        Gen.Int.Sample(
            seed =>
            {
                var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

                notes.Should().OnlyContain(note => scaleNoteNumbers.Contains(note.NoteNumber), "every pitch must be diatonic to the configured scale");
            },
            iter: SampleIterations
        );
    }

    [TestCase(2)]
    [TestCase(3)]
    public void ComposedNotes_InAModeWithoutALiftedGate_StayFullyDiatonic(int numberOfInstruments)
    {
        // The tonicization gate lifts mode by mode: in Dorian (and every other still-gated mode) no
        // chromatic license exists, so the strict all-diatonic invariant must keep holding exactly.
        var configuration = TestCompositionConfigurations.Get(numberOfInstruments, 10, tonic: NoteName.D, mode: Mode.Dorian) with { ShuffleOrnamentationProcessors = false };
        var scaleNoteNumbers = configuration.Scale.GetNotes().Select(static note => (int)note.NoteNumber).ToHashSet();

        Gen.Int.Sample(
            seed =>
            {
                var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

                notes.Should().OnlyContain(note => scaleNoteNumbers.Contains(note.NoteNumber), "every pitch must be diatonic to the configured scale");
            },
            iter: SampleIterations
        );
    }
}
