using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

internal sealed class IsApplicableInterval(CompositionConfiguration compositionConfiguration, int interval) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat?[item.Instrument];

        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);
        var nextNoteIndex = compositionConfiguration.Scale.IndexOf(nextNote ?? currentNote);

        return Math.Abs(nextNoteIndex - currentNoteIndex) == interval ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
