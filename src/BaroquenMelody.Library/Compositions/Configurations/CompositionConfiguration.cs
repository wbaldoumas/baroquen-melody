using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///     The composition configuration.
/// </summary>
/// <param name="VoiceConfigurations"> The voice configurations to be used in the composition. </param>
/// <param name="Scale"> The scale to be used in the composition. </param>
/// <param name="CompositionLength"> The length of the composition in measures. </param>
/// <param name="Meter"> The meter to be used in the composition. </param>
/// <param name="CompositionContextSize"> The size of the context to be used in the composition. </param>
internal sealed record CompositionConfiguration(
    ISet<VoiceConfiguration> VoiceConfigurations,
    Scale Scale,
    Meter Meter,
    int CompositionLength,
    int CompositionContextSize = 4)
{
    /// <summary>
    ///     Determine if the given note is within the range of the given voice for the composition.
    /// </summary>
    /// <param name="voice">The voice to check the note against.</param>
    /// <param name="note">The note to check against the voice.</param>
    /// <returns>Whether the note is within the range of the voice.</returns>
    public bool IsNoteInVoiceRange(Voice voice, Note note) => VoiceConfigurations.First(
        voiceConfiguration => voiceConfiguration.Voice == voice
    ).IsNoteWithinVoiceRange(note);
}
