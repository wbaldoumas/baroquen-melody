using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <inheritdoc cref="IInputPolicy{T}"/>
/// <remarks>
///     An appoggiatura is an accented dissonance, so the leaning tone one scale step above the target note
///     must genuinely clash with the sounding chord. When any voice already holds that pitch class, the
///     figure would merely double a chord tone in an ornament's rhythm, so the site is rejected.
/// </remarks>
internal sealed class LeaningToneIsDissonant(CompositionConfiguration compositionConfiguration) : IInputPolicy<OrnamentationItem>
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

        var leaningTone = compositionConfiguration.Scale.GetNotes()[currentNoteIndex + 1];

        return item.CurrentBeat.Chord.Notes.Exists(note => note.NoteName == leaningTone.NoteName)
            ? InputPolicyResult.Reject
            : InputPolicyResult.Continue;
    }
}
