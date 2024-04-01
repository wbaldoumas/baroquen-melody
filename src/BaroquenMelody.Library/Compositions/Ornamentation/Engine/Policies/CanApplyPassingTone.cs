using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;

internal sealed class CanApplyPassingTone(CompositionConfiguration configuration) : IInputPolicy<OrnamentationItem>
{
    private const int PassingToneThreshold = 2;

    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat?[item.Voice];

        var scaleNotes = configuration.Scale.GetNotes().ToList();

        var currentScaleIndex = scaleNotes.IndexOf(currentNote.Raw);
        var nextScaleIndex = scaleNotes.IndexOf(nextNote?.Raw ?? currentNote.Raw);

        return Math.Abs(nextScaleIndex - currentScaleIndex) == PassingToneThreshold ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
