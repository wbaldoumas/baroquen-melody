using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <inheritdoc cref="IInputPolicy{T}"/>
/// <remarks>
///     An appoggiatura's leaning tone must be struck fresh: when the same voice just sounded that very
///     pitch, the figure is a re-struck preparation - suspension territory, governed by the suspension
///     applicator's stricter guards - rather than the unprepared leaning gesture that defines the
///     appoggiatura. The approach is judged by the preceding beat's final sounded pitch, so an ornamented
///     approach is measured by what the listener actually hears.
/// </remarks>
internal sealed class LeaningToneIsNotRestruck(CompositionConfiguration compositionConfiguration) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        if (!item.CurrentBeat.ContainsInstrument(item.Instrument))
        {
            return InputPolicyResult.Reject;
        }

        var currentNote = item.CurrentBeat[item.Instrument];
        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);

        if (currentNoteIndex < 0)
        {
            return InputPolicyResult.Reject;
        }

        if (item.PrecedingBeats.Count == 0 || !item.PrecedingBeats[^1].ContainsInstrument(item.Instrument))
        {
            return InputPolicyResult.Continue;
        }

        var precedingNote = item.PrecedingBeats[^1][item.Instrument];
        var approachNote = precedingNote.Ornamentations.Count > 0 ? precedingNote.Ornamentations[^1] : precedingNote;
        var leaningTone = compositionConfiguration.Scale.GetNotes()[currentNoteIndex + 1];

        return approachNote.Raw == leaningTone
            ? InputPolicyResult.Reject
            : InputPolicyResult.Continue;
    }
}
