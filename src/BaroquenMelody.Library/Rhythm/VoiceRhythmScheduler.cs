using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Rhythm;

/// <inheritdoc cref="IVoiceRhythmScheduler"/>
/// <remarks>
///     Deterministic by design (no randomness) in both modes. ROTATION mode: roles are a pure function of the
///     measure index, the phrase-length block grid, and the register-ordered instrument list, rotating one
///     voice per block so every voice takes every role in turn. The held role exists only on top of the
///     harmonic-rhythm grid: its audible definition is one attack per measure, realized by pinning the single
///     interior beat that grid leaves fresh, so held queries answer nothing when harmonic rhythm is disabled,
///     while the florid role is grid-independent. Seam measures take no holds, and holding needs at least
///     three voices. TEXTURE mode (a texture configured, the voice-rhythm master switch on, and at least two
///     voices): the rotation and its pins decline entirely, and one static register-derived assignment stands
///     for the whole composition - the highest voice is the melody, the lowest carries the figuration, and
///     every voice between is a pad. The texture ordering breaks equal-floor ties by the higher ceiling and
///     then by instrument, so a tie can never let raw set order pick the figuration voice. Texture
///     deliberately does not require the harmonic-rhythm grid: pads record held without pins, tying
///     opportunistically wherever the free walk repeats a pitch and re-striking elsewhere - both are the
///     chordal idiom.
/// </remarks>
internal sealed class VoiceRhythmScheduler(CompositionConfiguration compositionConfiguration) : IVoiceRhythmScheduler
{
    private const int MinimumVoicesForHeldRole = 3;

    private const int MinimumVoicesForFloridRole = 2;

    private const int MinimumVoicesForTexture = 2;

    private readonly VoiceRhythmConfiguration _voiceRhythmConfiguration =
        compositionConfiguration.VoiceRhythmConfiguration ?? VoiceRhythmConfiguration.Default;

    private readonly HarmonicRhythmConfiguration _harmonicRhythmConfiguration =
        compositionConfiguration.HarmonicRhythmConfiguration ?? HarmonicRhythmConfiguration.Default;

    private readonly TextureConfiguration _textureConfiguration =
        compositionConfiguration.TextureConfiguration ?? TextureConfiguration.Default;

    private readonly List<Instrument> _textureOrderedInstruments = compositionConfiguration.InstrumentConfigurations
        .OrderByDescending(static instrumentConfiguration => instrumentConfiguration.MinNote)
        .ThenByDescending(static instrumentConfiguration => instrumentConfiguration.MaxNote)
        .ThenBy(static instrumentConfiguration => instrumentConfiguration.Instrument)
        .Select(static instrumentConfiguration => instrumentConfiguration.Instrument)
        .ToList();

    private bool IsTextureActive => _voiceRhythmConfiguration.Enabled
                                    && _textureConfiguration.Texture != TextureType.None
                                    && _textureOrderedInstruments.Count >= MinimumVoicesForTexture;

    public bool TryGetHeldInstrument(int measureIndex, out Instrument heldInstrument)
    {
        heldInstrument = default;

        var instruments = compositionConfiguration.Instruments;

        if (IsTextureActive
            || !_voiceRhythmConfiguration.Enabled
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

        if (IsTextureActive || !_voiceRhythmConfiguration.Enabled || instruments.Count < MinimumVoicesForFloridRole)
        {
            return false;
        }

        floridInstrument = instruments[(GetBlockIndex(measureIndex) + 1) % instruments.Count];

        return true;
    }

    public bool TryGetTextureDecorationOrder(out IReadOnlyList<Instrument> decorationOrder)
    {
        decorationOrder = [];

        if (!IsTextureActive)
        {
            return false;
        }

        decorationOrder = _textureOrderedInstruments;

        return true;
    }

    public bool TryGetTextureRole(Instrument instrument, out TextureRole textureRole)
    {
        textureRole = default;

        if (!IsTextureActive || !_textureOrderedInstruments.Contains(instrument))
        {
            return false;
        }

        textureRole = instrument == _textureOrderedInstruments[0]
            ? TextureRole.Melody
            : instrument == _textureOrderedInstruments[^1]
                ? TextureRole.Figuration
                : TextureRole.Pad;

        return true;
    }

    private bool IsSeamMeasure(int measureIndex) => measureIndex % compositionConfiguration.PhrasingConfiguration.MinPhraseLength == 0;

    private int GetBlockIndex(int measureIndex) => measureIndex / compositionConfiguration.PhrasingConfiguration.MinPhraseLength;
}
