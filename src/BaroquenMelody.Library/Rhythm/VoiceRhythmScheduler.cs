using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Rhythm;

/// <inheritdoc cref="IVoiceRhythmScheduler"/>
/// <remarks>
///     Deterministic by design (no randomness): roles are a pure function of the measure index, the
///     phrase-length block grid, and the register-ordered instrument list, rotating one voice per block so
///     every voice takes every role in turn. The held role exists only on top of the harmonic-rhythm grid:
///     its audible definition is one attack per measure, realized by pinning the single interior beat that
///     grid leaves fresh, so with the grid disabled the role would either evaporate (the voice moves freely
///     at the grid's held beats) or triple its pinned searches per measure — held queries therefore answer
///     nothing when harmonic rhythm is disabled, while the florid role is grid-independent. Seam measures
///     take no holds: they are the phraser's cadence-active measures, and the held voice composing freely
///     into the cadence is the idiom. Holding also needs at least three voices — with two, a held voice
///     would leave a single moving line rather than a texture.
/// </remarks>
internal sealed class VoiceRhythmScheduler(CompositionConfiguration compositionConfiguration) : IVoiceRhythmScheduler
{
    private const int MinimumVoicesForHeldRole = 3;

    private const int MinimumVoicesForFloridRole = 2;

    private readonly VoiceRhythmConfiguration _voiceRhythmConfiguration =
        compositionConfiguration.VoiceRhythmConfiguration ?? VoiceRhythmConfiguration.Default;

    private readonly HarmonicRhythmConfiguration _harmonicRhythmConfiguration =
        compositionConfiguration.HarmonicRhythmConfiguration ?? HarmonicRhythmConfiguration.Default;

    public bool TryGetHeldInstrument(int measureIndex, out Instrument heldInstrument)
    {
        heldInstrument = default;

        var instruments = compositionConfiguration.Instruments;

        if (!_voiceRhythmConfiguration.Enabled
            || !_harmonicRhythmConfiguration.Enabled
            || instruments.Count < MinimumVoicesForHeldRole
            || IsSeamMeasure(measureIndex))
        {
            return false;
        }

        heldInstrument = instruments[GetBlockIndex(measureIndex) % instruments.Count];

        return true;
    }

    public bool TryGetPinnedInstrument(int measureIndex, int beatIndex, out Instrument pinnedInstrument)
    {
        pinnedInstrument = default;

        // The harmonic-rhythm grid holds the odd beats of non-seam measures and the walk composes beat zero
        // as the held voice's one attack, so the only beats left to pin are the later even ones.
        return beatIndex > 0 && beatIndex % 2 == 0 && TryGetHeldInstrument(measureIndex, out pinnedInstrument);
    }

    public bool TryGetFloridInstrument(int measureIndex, out Instrument floridInstrument)
    {
        floridInstrument = default;

        var instruments = compositionConfiguration.Instruments;

        if (!_voiceRhythmConfiguration.Enabled || instruments.Count < MinimumVoicesForFloridRole)
        {
            return false;
        }

        floridInstrument = instruments[(GetBlockIndex(measureIndex) + 1) % instruments.Count];

        return true;
    }

    private bool IsSeamMeasure(int measureIndex) => measureIndex % compositionConfiguration.PhrasingConfiguration.MinPhraseLength == 0;

    private int GetBlockIndex(int measureIndex) => measureIndex / compositionConfiguration.PhrasingConfiguration.MinPhraseLength;
}
