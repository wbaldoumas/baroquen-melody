using BaroquenMelody.Library.Compositions.Configurations;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <summary>
///     Generates the possible note contexts for the given voice.
/// </summary>
internal interface INoteContextGenerator
{
    /// <summary>
    ///     Generate the possible note contexts for the given voice configuration and scale.
    /// </summary>
    /// <param name="voiceConfiguration"> The voice configuration to generate note contexts for. </param>
    /// <param name ="scale"> The scale to be used in the generation of note contexts. </param>
    /// <returns> The possible note contexts for the given voice configuration and scale. </returns>
    ISet<NoteContext> GenerateNoteContexts(VoiceConfiguration voiceConfiguration, Scale scale);
}
