using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///     The composition configuration.
/// </summary>
/// <param name="VoiceConfigurations"> The voice configurations to be used in the composition. </param>
/// <param name="Scale"> The scale to be used in the composition. </param>
/// <param name="CompositionLength"> The length of the composition in ticks. </param>
/// <param name="Meter"> The meter to be used in the composition. </param>
internal sealed record CompositionConfiguration(
    ISet<VoiceConfiguration> VoiceConfigurations,
    Scale Scale,
    Meter Meter,
    int CompositionLength)
{
    public bool IsNoteInVoiceRange(Voice voice, Note note) => VoiceConfigurations.First(
        voiceConfiguration => voiceConfiguration.Voice == voice
    ).IsNoteWithinVoiceRange(note);
}
