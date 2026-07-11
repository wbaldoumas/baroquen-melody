using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Rules.Harmonic;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Rules;

/// <inheritdoc cref="IVoiceSpacingSatisfiabilityAnalyzer"/>
/// <remarks>
///     Voices are ordered from highest register to lowest, mirroring <see cref="CompositionConfiguration.Instruments"/>,
///     and the spacing constraints form a path over the adjacent upper-voice pairs (the lowest pair is unrestricted).
///     A single top-down pass propagating each voice's feasible note set is therefore exact: the rule is satisfiable
///     if and only if every constrained voice retains at least one feasible note. A constrained voice with no playable
///     scale notes at all also yields <see langword="false"/>, since no chord could be voiced in the first place.
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
        var voices = instrumentConfigurations
            .OrderByDescending(static instrumentConfiguration => instrumentConfiguration.MinNote)
            .ToList();

        if (voices.Count < MinimumConstrainedVoiceCount)
        {
            return true;
        }

        var scaleNotes = scale.GetNotes();
        var feasibleNotes = GetPlayableNotes(scaleNotes, voices[0]);

        // skip the lowest adjacent pair (the final index) since bass-tenor spacing is unrestricted.
        for (var voiceIndex = 1; voiceIndex <= voices.Count - 2; ++voiceIndex)
        {
            feasibleNotes = GetPlayableNotes(scaleNotes, voices[voiceIndex])
                .Where(playableNote => feasibleNotes.Exists(feasibleNote => Math.Abs((int)playableNote.NoteNumber - (int)feasibleNote.NoteNumber) <= EnforceVoiceSpacing.MaximumAdjacentVoiceSpacing))
                .ToList();

            if (feasibleNotes.Count == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static List<Note> GetPlayableNotes(List<Note> scaleNotes, InstrumentConfiguration instrumentConfiguration) =>
        scaleNotes.Where(instrumentConfiguration.IsNoteWithinInstrumentRange).ToList();
}
