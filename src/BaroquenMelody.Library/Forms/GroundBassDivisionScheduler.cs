using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Forms;

/// <inheritdoc cref="IGroundBassDivisionScheduler"/>
/// <remarks>
///     Deterministic by design (no randomness): every answer is a pure function of the plan and the
///     statement index. The intensity ramp is anchored at the END — the final accompanied statement takes
///     the peak at every statement count, because a start-anchored ramp would compose a two-statement
///     plan's only accompanied statement calm and quiet, inverting the arc the divisions idiom promises.
///     The opening announcement (statement zero) never escalates, and a ground with no upper voices has no
///     division voice to escalate, so only the ground line's sustained tread applies there. Divisions ride
///     the voice-rhythm role machinery, so <see cref="VoiceRhythmConfiguration.Enabled"/> is the master
///     switch and <see cref="GroundBassConfiguration.Divisions"/> narrows it for this form.
/// </remarks>
internal sealed class GroundBassDivisionScheduler(CompositionConfiguration compositionConfiguration) : IGroundBassDivisionScheduler
{
    internal const int CalmIntensity = 30;

    internal const int PeakIntensity = 140;

    internal const int TerraceDivisor = 20;

    // Counts every voice, the ground line included: the planner always draws the bass from Instruments,
    // so two voices guarantee the non-empty upper-voice list the florid rotation indexes into.
    private const int MinimumVoicesForEscalation = 2;

    private readonly bool _isActive =
        (compositionConfiguration.VoiceRhythmConfiguration ?? VoiceRhythmConfiguration.Default).Enabled
        && (compositionConfiguration.GroundBassConfiguration ?? GroundBassConfiguration.Default).Divisions;

    public bool ShouldHoldGroundLine() => _isActive;

    public bool TryGetIntensity(GroundBassPlan plan, int statementIndex, out int intensity)
    {
        intensity = default;

        if (!CanEscalate(statementIndex))
        {
            return false;
        }

        intensity = PeakIntensity - (PeakIntensity - CalmIntensity) * (plan.StatementCount - 1 - statementIndex) / Math.Max(1, plan.StatementCount - 2);

        return true;
    }

    public bool TryGetFloridInstrument(GroundBassPlan plan, int statementIndex, out Instrument floridInstrument)
    {
        floridInstrument = default;

        if (!CanEscalate(statementIndex))
        {
            return false;
        }

        var upperVoices = compositionConfiguration.Instruments.Where(instrument => instrument != plan.BassInstrument).ToList();

        floridInstrument = upperVoices[(statementIndex - 1) % upperVoices.Count];

        return true;
    }

    public bool TryGetTerraceOffset(GroundBassPlan plan, int statementIndex, out int terraceOffset)
    {
        terraceOffset = default;

        if (!TryGetIntensity(plan, statementIndex, out var intensity))
        {
            return false;
        }

        terraceOffset = (intensity - PeakIntensity) / TerraceDivisor;

        return true;
    }

    private bool CanEscalate(int statementIndex) =>
        _isActive
        && statementIndex > 0
        && compositionConfiguration.Instruments.Count >= MinimumVoicesForEscalation;
}
