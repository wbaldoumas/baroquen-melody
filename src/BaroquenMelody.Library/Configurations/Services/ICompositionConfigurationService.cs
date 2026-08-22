using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Configurations.Services;

/// <summary>
///     A service that deals with the configuration of compositions.
/// </summary>
public interface ICompositionConfigurationService
{
    /// <summary>
    ///     The root notes can be configured by the user (e.g. "C", "F#", "G#", etc.).
    /// </summary>
    IEnumerable<NoteName> ConfigurableRootNotes { get; }

    /// <summary>
    ///     The scale modes that can be configured by the user (e.g. "Ionian", "Dorian", "Phrygian", etc.).
    /// </summary>
    IEnumerable<Mode> ConfigurableScaleModes { get; }

    /// <summary>
    ///     The meters that can be configured by the user (e.g. "4/4", "3/4", etc.).
    /// </summary>
    IEnumerable<Meter> ConfigurableMeters { get; }

    /// <summary>
    ///     The large-scale forms that can be configured by the user (e.g. "Fugue", "Ground Bass").
    /// </summary>
    IEnumerable<CompositionForm> ConfigurableCompositionForms { get; }

    /// <summary>
    ///     The ground bass patterns that can be configured by the user: <see langword="null"/> for the
    ///     composer's free draw among the patterns that fit, then the bank's patterns in bank order.
    /// </summary>
    IEnumerable<GroundBass?> ConfigurableGroundBassPatterns { get; }

    /// <summary>
    ///     The accompaniment textures that can be configured by the user for the fugue form
    ///     (e.g. "None", "Chordal", "Walking", "Broken Chord").
    /// </summary>
    IEnumerable<TextureType> ConfigurableTextures { get; }

    /// <summary>
    ///     Randomize the composition configuration.
    /// </summary>
    void Randomize();

    /// <summary>
    ///     Reset the composition configuration to its default state.
    /// </summary>
    void Reset();
}
