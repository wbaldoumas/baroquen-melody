using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library;

/// <summary>
///     Configures a new <see cref="IMidiFileComposer"/> which can compose a <see cref="MidiFileComposition"/> for the given <see cref="CompositionConfiguration"/>.
/// </summary>
public interface IBaroquenMelodyComposerConfigurator
{
    /// <summary>
    ///     Configure a new <see cref="IMidiFileComposer"/> with the given <see cref="CompositionConfiguration"/>.
    /// </summary>
    /// <param name="compositionConfiguration">The <see cref="CompositionConfiguration"/> to configure the <see cref="IMidiFileComposer"/> with.</param>
    /// <returns>The configured <see cref="IMidiFileComposer"/>.</returns>
    IMidiFileComposer Configure(CompositionConfiguration compositionConfiguration);
}
