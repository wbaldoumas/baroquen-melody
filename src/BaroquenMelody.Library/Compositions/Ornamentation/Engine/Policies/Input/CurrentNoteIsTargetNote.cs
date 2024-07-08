﻿using Atrea.PolicyEngine.Policies.Input;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class CurrentNoteIsTargetNote(NoteName noteName) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => item.CurrentBeat[item.Voice].Raw.NoteName == noteName ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
