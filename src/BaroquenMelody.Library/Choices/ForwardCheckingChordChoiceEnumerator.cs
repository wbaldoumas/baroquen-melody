using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Extensions;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Choices;

/// <inheritdoc cref="IChordChoiceEnumerator"/>
/// <remarks>
///     Forward checking: before enumerating the Cartesian product of per-voice note choices, each sounding voice's
///     domain is pruned to the choices that land within the instrument's playable range. Range violations are a hard,
///     non-bypassable gate (the aggregate rule evaluates <see cref="Rules.EnsureInstrumentRange"/> first, before any
///     strictness bypass can draw randomness), so pruned candidates could never have passed nor consumed a random draw
///     — the surviving sequence, and therefore the composed music, is identical to enumerating the full repository.
///     Voices absent from the current chord (a partially voiced fugal exposition) cannot be range-checked and keep
///     their full domains; their choices vary the <see cref="ChordChoice"/> but contribute no note, matching
///     <see cref="BaroquenChordExtensions.ApplyChordChoice"/>.
/// </remarks>
internal sealed class ForwardCheckingChordChoiceEnumerator : IChordChoiceEnumerator
{
    private readonly CompositionConfiguration _compositionConfiguration;

    private readonly Instrument[] _instruments;

    private readonly NoteChoice[][] _noteChoicesByVoice;

    public ForwardCheckingChordChoiceEnumerator(CompositionConfiguration compositionConfiguration, INoteChoiceGenerator noteChoiceGenerator)
    {
        _compositionConfiguration = compositionConfiguration;

        // Match the chord-choice repositories: voices in instrument-enum order, each voice's choices in the
        // generator's enumeration order, so candidates come out in the exact repository sequence.
        _instruments = [.. compositionConfiguration.Instruments.Order()];
        _noteChoicesByVoice = [.. _instruments.Select(instrument => noteChoiceGenerator.GenerateNoteChoices(instrument).ToArray())];
    }

    public IEnumerable<(ChordChoice ChordChoice, BaroquenChord Chord)> EnumerateCandidates(BaroquenChord currentChord)
    {
        var voiceDomains = new List<(NoteChoice NoteChoice, Note? TargetRawNote)>[_instruments.Length];

        for (var voiceIndex = 0; voiceIndex < _instruments.Length; ++voiceIndex)
        {
            var voiceDomain = GetViableNoteChoices(currentChord, voiceIndex);

            if (voiceDomain.Count == 0)
            {
                yield break;
            }

            voiceDomains[voiceIndex] = voiceDomain;
        }

        // Odometer over the pruned domains in mixed-radix order (last voice varies fastest), mirroring the lazy
        // Cartesian product's index decomposition.
        var domainIndexes = new int[voiceDomains.Length];

        while (true)
        {
            yield return BuildCandidate(voiceDomains, domainIndexes);

            var voiceIndex = voiceDomains.Length - 1;

            while (voiceIndex >= 0 && ++domainIndexes[voiceIndex] == voiceDomains[voiceIndex].Count)
            {
                domainIndexes[voiceIndex] = 0;
                --voiceIndex;
            }

            if (voiceIndex < 0)
            {
                yield break;
            }
        }
    }

    private List<(NoteChoice NoteChoice, Note? TargetRawNote)> GetViableNoteChoices(BaroquenChord currentChord, int voiceIndex)
    {
        var instrument = _instruments[voiceIndex];
        var noteChoices = _noteChoicesByVoice[voiceIndex];
        var voiceDomain = new List<(NoteChoice NoteChoice, Note? TargetRawNote)>(noteChoices.Length);

        if (!currentChord.ContainsInstrument(instrument))
        {
            voiceDomain.AddRange(noteChoices.Select(static noteChoice => (noteChoice, (Note?)null)));

            return voiceDomain;
        }

        var currentNote = currentChord[instrument];

        foreach (var noteChoice in noteChoices)
        {
            var targetRawNote = currentNote.GetTargetRawNote(_compositionConfiguration.Scale, noteChoice);

            if (_compositionConfiguration.IsNoteInInstrumentRange(instrument, targetRawNote))
            {
                voiceDomain.Add((noteChoice, targetRawNote));
            }
        }

        return voiceDomain;
    }

    private (ChordChoice ChordChoice, BaroquenChord Chord) BuildCandidate(
        List<(NoteChoice NoteChoice, Note? TargetRawNote)>[] voiceDomains,
        int[] domainIndexes)
    {
        var noteChoices = new NoteChoice[voiceDomains.Length];
        var notes = new List<BaroquenNote>(voiceDomains.Length);

        for (var voiceIndex = 0; voiceIndex < voiceDomains.Length; ++voiceIndex)
        {
            var (noteChoice, targetRawNote) = voiceDomains[voiceIndex][domainIndexes[voiceIndex]];

            noteChoices[voiceIndex] = noteChoice;

            if (targetRawNote is not null)
            {
                notes.Add(new BaroquenNote(_instruments[voiceIndex], targetRawNote, _compositionConfiguration.DefaultNoteTimeSpan));
            }
        }

        return (new ChordChoice(noteChoices), new BaroquenChord(notes));
    }
}
