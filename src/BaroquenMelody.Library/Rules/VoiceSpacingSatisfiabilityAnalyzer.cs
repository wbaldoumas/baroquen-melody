using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules.Harmonic;
using System.Collections.Frozen;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Rules;

/// <inheritdoc cref="IVoiceSpacingSatisfiabilityAnalyzer"/>
/// <remarks>
///     Voices are ordered from highest register to lowest, mirroring <see cref="CompositionConfiguration.Instruments"/>,
///     and the spacing constraints form a path over the adjacent upper-voice pairs (the lowest pair is unrestricted).
///     A single top-down pass propagating each voice's feasible note set decides satisfiability exactly; the
///     complementary bottom-up pass makes every voice's set arc-consistent, so a note is kept if and only if some
///     full voicing satisfying the rule contains it. A constrained voice with no playable scale notes at all yields
///     <see langword="false"/>, since no chord could be voiced in the first place.
/// </remarks>
public sealed class VoiceSpacingSatisfiabilityAnalyzer : IVoiceSpacingSatisfiabilityAnalyzer
{
    private const int MinimumConstrainedVoiceCount = 3;

    public bool IsSatisfiable(CompositionConfiguration compositionConfiguration) => IsSatisfiable(
        compositionConfiguration.InstrumentConfigurations,
        compositionConfiguration.Scale
    );

    public bool IsSatisfiable(IEnumerable<InstrumentConfiguration> instrumentConfigurations, BaroquenScale scale)
    {
        var voices = OrderByRegister(instrumentConfigurations);

        if (voices.Count < MinimumConstrainedVoiceCount)
        {
            return true;
        }

        var feasibleNotesByVoice = ComputeFeasibleNotesByVoice(voices, scale);

        // the constraint path covers every voice but the bass, whose adjacent pair is unrestricted.
        return feasibleNotesByVoice.Take(voices.Count - 1).All(static feasibleNotes => feasibleNotes.Count > 0);
    }

    public FrozenDictionary<Instrument, FrozenSet<int>> GetFeasibleNoteNumbersByInstrument(CompositionConfiguration compositionConfiguration)
    {
        var voices = OrderByRegister(compositionConfiguration.InstrumentConfigurations);
        var scaleNotes = compositionConfiguration.Scale.GetNotes();

        var feasibleNotesByVoice = voices.Count < MinimumConstrainedVoiceCount
            ? voices.Select(voice => GetPlayableNotes(scaleNotes, voice)).ToList()
            : ComputeFeasibleNotesByVoice(voices, compositionConfiguration.Scale);

        return voices
            .Select((voice, voiceIndex) => (
                voice.Instrument,
                FeasibleNoteNumbers: feasibleNotesByVoice[voiceIndex].Select(static note => (int)note.NoteNumber).ToFrozenSet()
            ))
            .ToFrozenDictionary(static pair => pair.Instrument, static pair => pair.FeasibleNoteNumbers);
    }

    private static List<List<Note>> ComputeFeasibleNotesByVoice(List<InstrumentConfiguration> voices, BaroquenScale scale)
    {
        var scaleNotes = scale.GetNotes();
        var feasibleNotesByVoice = voices.Select(voice => GetPlayableNotes(scaleNotes, voice)).ToList();

        // The constrained pairs (i, i+1) for i < Count-2 form a path over the upper voices; the lowest adjacent
        // pair (bass and tenor) is unrestricted. One top-down and one bottom-up pass over a path make each set
        // arc-consistent: every kept note extends to a full rule-satisfying voicing.
        var lowestConstrainedVoiceIndex = voices.Count - 2;

        for (var voiceIndex = 1; voiceIndex <= lowestConstrainedVoiceIndex; ++voiceIndex)
        {
            feasibleNotesByVoice[voiceIndex] = FilterToNotesWithinSpacing(feasibleNotesByVoice[voiceIndex], feasibleNotesByVoice[voiceIndex - 1]);
        }

        for (var voiceIndex = lowestConstrainedVoiceIndex - 1; voiceIndex >= 0; --voiceIndex)
        {
            feasibleNotesByVoice[voiceIndex] = FilterToNotesWithinSpacing(feasibleNotesByVoice[voiceIndex], feasibleNotesByVoice[voiceIndex + 1]);
        }

        return feasibleNotesByVoice;
    }

    private static List<Note> FilterToNotesWithinSpacing(List<Note> notes, List<Note> neighborNotes) => notes
        .Where(note => neighborNotes.Exists(neighborNote => Math.Abs((int)note.NoteNumber - (int)neighborNote.NoteNumber) <= EnforceVoiceSpacing.MaximumAdjacentVoiceSpacing))
        .ToList();

    private static List<InstrumentConfiguration> OrderByRegister(IEnumerable<InstrumentConfiguration> instrumentConfigurations) => instrumentConfigurations
        .OrderByDescending(static instrumentConfiguration => instrumentConfiguration.MinNote)
        .ToList();

    private static List<Note> GetPlayableNotes(List<Note> scaleNotes, InstrumentConfiguration instrumentConfiguration) =>
        scaleNotes.Where(instrumentConfiguration.IsNoteWithinInstrumentRange).ToList();
}
