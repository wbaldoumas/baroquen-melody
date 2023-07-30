using BaroquenMelody.Library.Composition.Configurations;

namespace BaroquenMelody.Library.Composition.Contexts;

/// <summary>
///     Generates the possible note contexts for the given voice.
/// </summary>
internal interface INoteContextGenerator
{
    /// <summary>
    ///     Generate the possible note contexts for the given voice configuration.
    /// </summary>
    /// <param name="voiceConfiguration"> The voice configuration to generate note contexts for. </param>
    /// <returns> The possible note contexts for the given voice configuration. </returns>
    ISet<NoteContext> GenerateNoteContexts(VoiceConfiguration voiceConfiguration);
}
