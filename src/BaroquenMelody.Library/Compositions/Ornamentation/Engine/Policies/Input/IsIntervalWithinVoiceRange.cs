using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class IsIntervalWithinVoiceRange(CompositionConfiguration compositionConfiguration, int interval) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var notes = compositionConfiguration.Scale.GetNotes();
        var noteIndex = notes.IndexOf(currentNote.Raw);
        var nextNote = notes[noteIndex + interval];

        return compositionConfiguration.IsNoteInVoiceRange(item.Voice, nextNote) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
