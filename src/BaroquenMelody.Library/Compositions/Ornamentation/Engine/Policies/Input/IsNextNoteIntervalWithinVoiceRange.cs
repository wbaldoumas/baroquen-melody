﻿using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class IsNextNoteIntervalWithinVoiceRange(CompositionConfiguration compositionConfiguration, int interval) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var nextNote = item.NextBeat![item.Voice];
        var noteIndex = compositionConfiguration.Scale.IndexOf(nextNote);

        var notes = compositionConfiguration.Scale.GetNotes();
        var intervalNote = notes[noteIndex + interval];

        return compositionConfiguration.IsNoteInVoiceRange(item.Voice, intervalNote) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
