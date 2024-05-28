using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;

internal sealed class IsApplicableInterval(CompositionConfiguration configuration, int Interval) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat?[item.Voice];

        var notes = configuration.Scale.GetNotes().ToList();

        var currentNoteScaleIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteScaleIndex = notes.IndexOf(nextNote?.Raw ?? currentNote.Raw);

        return Math.Abs(nextNoteScaleIndex - currentNoteScaleIndex) == Interval ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
