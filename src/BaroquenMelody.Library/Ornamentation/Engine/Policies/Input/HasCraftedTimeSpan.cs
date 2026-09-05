using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <summary>
///     Continues when either note of the pair carries a crafted time span: a span hand-set away from the
///     composition's default with no ornamentation stamp accounting for it - today only the ending's
///     whole-note final chord. Negated in the sustain engine, this keeps the tie from rewriting the crafted
///     close (truncating it forward into its rest, or silencing it backward under an extended predecessor),
///     while a stamped non-default note - a suspension preparation, a figured principal - stays absorbable.
/// </summary>
internal sealed class HasCraftedTimeSpan(CompositionConfiguration compositionConfiguration) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        HasCraftedNote(item.CurrentBeat, item.Instrument) || HasCraftedNote(item.NextBeat, item.Instrument)
            ? InputPolicyResult.Continue
            : InputPolicyResult.Reject;

    private bool HasCraftedNote(Beat? beat, Instrument instrument)
    {
        if (beat is null || !beat.ContainsInstrument(instrument))
        {
            return false;
        }

        var note = beat[instrument];

        return note.OrnamentationType == OrnamentationType.None && note.MusicalTimeSpan != compositionConfiguration.DefaultNoteTimeSpan;
    }
}
