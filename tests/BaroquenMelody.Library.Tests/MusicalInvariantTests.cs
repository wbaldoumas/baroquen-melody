using BaroquenMelody.Library.Tests.TestData;
using CsCheck;
using FluentAssertions;
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
        var configuration = TestCompositionConfigurations.Get(numberOfInstruments, 10) with { ShuffleOrnamentationProcessors = false };
        var scaleNoteNumbers = configuration.Scale.GetNotes().Select(static note => (int)note.NoteNumber).ToHashSet();
        var minVelocity = configuration.InstrumentConfigurations.Min(static instrument => (int)instrument.MinVelocity);
        var maxVelocity = configuration.InstrumentConfigurations.Max(static instrument => (int)instrument.MaxVelocity);

        Gen.Int.Sample(
            seed =>
            {
                var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

                notes.Should().NotBeEmpty("every composition must produce notes");
                notes.Should().OnlyContain(note => scaleNoteNumbers.Contains(note.NoteNumber), "every pitch must be diatonic to the configured scale");
                notes.Should().OnlyContain(note => note.Velocity >= minVelocity && note.Velocity <= maxVelocity, "velocity must stay within the configured dynamic range");
                notes.Should().OnlyContain(note => note.NoteNumber >= 0 && note.NoteNumber <= 127, "every note number must be a valid MIDI pitch");
            },
            iter: SampleIterations
        );
    }
}
