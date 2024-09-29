using BaroquenMelody.Library.Enums;
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
    ///     Randomize the composition configuration.
    /// </summary>
    void Randomize();

    /// <summary>
    ///     Reset the composition configuration to its default state.
    /// </summary>
    void Reset();
}
